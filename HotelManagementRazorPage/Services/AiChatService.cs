using BusinessObjects.Entities;
using Microsoft.Extensions.Configuration;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IRoomRepository _roomRepo;
        private readonly string _apiKey;

        // Danh sách model ưu tiên (free tier quota cao nhất → ít trước nhất)
        private static readonly string[] _preferredModels =
        [
            "gemini-1.5-flash-8b",  // free tier quota cao nhất
            "gemini-1.5-flash",
            "gemini-2.0-flash-lite",
            "gemini-2.0-flash",
            "gemini-pro"
        ];
        private static string? _cachedModel;
        // Model → thời điểm hết cooldown (thay vì blacklist vĩnh viễn)
        private static readonly Dictionary<string, DateTime> _modelCooldowns = new();
        private static readonly TimeSpan _cooldownDuration = TimeSpan.FromMinutes(2);
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static readonly HttpClient _http = new();
        // Rate limiting: tối đa 1 request / giây
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(1);

        public AiChatService(IRoomRepository roomRepo, IConfiguration config)
        {
            _roomRepo = roomRepo;
            _apiKey   = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey chưa được cấu hình.");
        }

        // Kiểm tra model có đang trong cooldown không
        private static bool IsModelOnCooldown(string model) =>
            _modelCooldowns.TryGetValue(model, out var until) && DateTime.UtcNow < until;

        // ── Tự động chọn model phù hợp (cooldown thay vì blacklist vĩnh viễn)
        private async Task<string?> GetModelAsync()
        {
            if (_cachedModel != null && !IsModelOnCooldown(_cachedModel))
                return _cachedModel;

            await _lock.WaitAsync();
            try
            {
                // Kiểm tra lại sau khi có lock
                if (_cachedModel != null && !IsModelOnCooldown(_cachedModel))
                    return _cachedModel;

                _cachedModel = null; // reset để chọn lại

                // Lấy danh sách model available từ API
                List<string> available = new();
                try
                {
                    string listUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}";
                    using var resp = await _http.GetAsync(listUrl);
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        available = doc.RootElement
                            .GetProperty("models")
                            .EnumerateArray()
                            .Where(m =>
                            {
                                bool canGenerate = m.TryGetProperty("supportedGenerationMethods", out var methods) &&
                                                   methods.EnumerateArray().Any(x => x.GetString() == "generateContent");
                                string name = m.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                                return canGenerate && !name.Contains("embed") && !name.Contains("vision");
                            })
                            .Select(m =>
                            {
                                string n = m.GetProperty("name").GetString()!;
                                return n.StartsWith("models/") ? n["models/".Length..] : n;
                            })
                            .ToList();
                    }
                }
                catch { /* ignore, sử dụng preferred list */ }

                // Chọn theo ưu tiên, dùng TÊN THỰC TẾ từ available list
                foreach (var pref in _preferredModels)
                {
                    if (IsModelOnCooldown(pref)) continue;

                    if (available.Any())
                    {
                        // Tìm tên thực tế (vd: "gemini-1.5-flash-8b-001") khớp với pref
                        string? actualName = available.FirstOrDefault(a =>
                            a.Contains(pref) && !IsModelOnCooldown(a));
                        if (actualName != null) { _cachedModel = actualName; return _cachedModel; }
                    }
                    else
                    {
                        // Không fetch được list → thử trực tiếp bằng pref keyword
                        _cachedModel = pref; return _cachedModel;
                    }
                }

                // Fallback: model available nào chưa cooldown
                string? fallback = available.FirstOrDefault(a => !IsModelOnCooldown(a));
                _cachedModel = fallback;
                return _cachedModel;
            }
            finally { _lock.Release(); }
        }

        public async Task<ChatResponse> SendMessageAsync(string userMessage, List<ChatMessage> history)
        {
            var rooms = _roomRepo.GetAll();

            // ── System prompt với dữ liệu phòng thực ───────────────────
            var sb = new StringBuilder();
            foreach (var r in rooms)
            {
                sb.AppendLine(
                    $"- ID:{r.Id} | Phòng {r.RoomNumber} | Loại: {r.RoomType?.Name ?? "N/A"} " +
                    $"| Giá: {r.RoomType?.PricePerNight ?? 0:N0}₫/đêm " +
                    $"| Sức chứa: {r.MaxOccupancy} người " +
                    $"| Mô tả: {(string.IsNullOrEmpty(r.Description) ? "Phòng tiêu chuẩn" : r.Description)}");
            }

            string systemPrompt =
                "Bạn là trợ lý AI của khách sạn Mường Thanh Hotel. " +
                "Nhiệm vụ của bạn là tư vấn, gợi ý phòng phù hợp cho khách hàng dựa trên thông tin thực tế dưới đây. " +
                "Hãy trả lời ngắn gọn, thân thiện bằng tiếng Việt. " +
                "Khi gợi ý phòng, hãy đề cập đến ID phòng theo định dạng [ROOM:id] (ví dụ: [ROOM:5]) để hệ thống hiển thị card phòng. " +
                "Gợi ý tối đa 4 phòng. Chỉ gợi ý phòng có trong danh sách.\n\n" +
                "DANH SÁCH PHÒNG:\n" + sb.ToString();

            // ── Xây dựng contents ────────────────────────────────────────
            var contents = new List<object>();
            foreach (var h in history.TakeLast(8))
            {
                contents.Add(new
                {
                    role  = h.Role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = h.Content } }
                });
            }
            contents.Add(new
            {
                role  = "user",
                parts = new[] { new { text = userMessage } }
            });

            // ── Gọi Gemini API (retry khi 429, thử model khác) ──────────
            string? model = await GetModelAsync();
            if (model == null)
                return new ChatResponse { Message = "⚠️ Không tìm được model Gemini. Vui lòng thử lại sau.", SuggestedRooms = new() };

            var requestBody = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents,
                generationConfig   = new { temperature = 0.7, maxOutputTokens = 800 }
            };
            string bodyJson = JsonSerializer.Serialize(requestBody);

            for (int attempt = 0; attempt < _preferredModels.Length; attempt++)
            {
                // Rate limiting: đảm bảo tối thiểu 1s giữa các request
                var elapsed = DateTime.UtcNow - _lastRequestTime;
                if (elapsed < _minRequestInterval)
                    await Task.Delay(_minRequestInterval - elapsed);
                _lastRequestTime = DateTime.UtcNow;

                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                try
                {
                    using var resp = await _http.SendAsync(req);
                    string raw = await resp.Content.ReadAsStringAsync();

                    if ((int)resp.StatusCode == 429)
                    {
                        // Đặt cooldown 2 phút, thử model kế tiếp với exponential backoff
                        _modelCooldowns[model] = DateTime.UtcNow.Add(_cooldownDuration);
                        _cachedModel = null;
                        int delayMs = 1000 * (int)Math.Pow(2, attempt); // 1s → 2s → 4s...
                        await Task.Delay(Math.Min(delayMs, 8000));
                        model = await GetModelAsync();
                        if (model == null) break;
                        continue;
                    }

                    if (!resp.IsSuccessStatusCode)
                        return new ChatResponse { Message = $"⚠️ Lỗi dịch vụ AI (HTTP {(int)resp.StatusCode}). Vui lòng thử lại.", SuggestedRooms = new() };

                    using var doc = JsonDocument.Parse(raw);
                    string aiText = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "Tôi chưa hiểu yêu cầu. Vui lòng thử lại.";

                    return new ChatResponse
                    {
                        Message        = CleanRoomTags(aiText),
                        SuggestedRooms = ExtractSuggestedRooms(aiText, rooms)
                    };
                }
                catch
                {
                    return new ChatResponse { Message = "⚠️ Không thể kết nối AI. Vui lòng thử lại sau.", SuggestedRooms = new() };
                }
            }

            return new ChatResponse { Message = "⚠️ Tất cả model AI đang quá tải. Vui lòng thử lại sau vài phút.", SuggestedRooms = new() };
        }

        private List<SuggestedRoom> ExtractSuggestedRooms(string text, List<Room> allRooms)
        {
            var result  = new List<SuggestedRoom>();
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\[ROOM:(\d+)\]");
            var seen    = new HashSet<int>();

            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                if (!int.TryParse(m.Groups[1].Value, out int id) || !seen.Add(id)) continue;
                var room = allRooms.FirstOrDefault(r => r.Id == id);
                if (room == null) continue;

                result.Add(new SuggestedRoom
                {
                    Id            = room.Id,
                    RoomNumber    = room.RoomNumber,
                    RoomType      = room.RoomType?.Name ?? "Phòng",
                    PricePerNight = room.RoomType?.PricePerNight ?? 0,
                    MaxOccupancy  = room.MaxOccupancy,
                    ImageUrl      = room.ImageUrl ?? "",
                    Reason        = ""
                });
                if (result.Count >= 4) break;
            }
            return result;
        }

        private static string CleanRoomTags(string text) =>
            System.Text.RegularExpressions.Regex.Replace(text, @"\[ROOM:\d+\]", "").Trim();
    }
}
