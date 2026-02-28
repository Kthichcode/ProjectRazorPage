using BusinessObjects.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace Services
{
    /// <summary>
    /// Smart keyword-based chat service — không cần external API.
    /// Phân tích tin nhắn tiếng Việt/Anh để tìm phòng phù hợp.
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private readonly IRoomRepository _roomRepo;

        public AiChatService(IRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        public async Task<ChatResponse> SendMessageAsync(string userMessage, List<ChatMessage> history)
        {
            await Task.CompletedTask; // giữ async signature

            var msg = userMessage.ToLower().Trim();
            var rooms = _roomRepo.GetAll();

            // ── Phân tích ý định ──────────────────────────────────────
            var intent = DetectIntent(msg);

            return intent switch
            {
                "greeting"     => HandleGreeting(),
                "available"    => HandleAvailable(rooms),
                "price"        => HandlePriceQuery(msg, rooms),
                "capacity"     => HandleCapacityQuery(msg, rooms),
                "room_type"    => HandleRoomTypeQuery(msg, rooms),
                "all_rooms"    => HandleAllRooms(rooms),
                "help"         => HandleHelp(),
                _              => HandleSmartSearch(msg, rooms)
            };
        }

        // ══════════════════════════════════════════════════════════════
        //  INTENT DETECTION
        // ══════════════════════════════════════════════════════════════

        private string DetectIntent(string msg)
        {
            if (IsGreeting(msg))    return "greeting";
            if (IsHelp(msg))        return "help";
            if (IsAvailable(msg))   return "available";
            if (HasPriceKeyword(msg)) return "price";
            if (HasCapacityKeyword(msg)) return "capacity";
            if (HasRoomTypeKeyword(msg)) return "room_type";
            if (IsListAll(msg))     return "all_rooms";
            return "smart_search";
        }

        private bool IsGreeting(string m) =>
            Regex.IsMatch(m, @"\b(xin chào|chào|hello|hi|hey|good morning|good afternoon)\b");

        private bool IsHelp(string m) =>
            Regex.IsMatch(m, @"\b(giúp|help|hướng dẫn|tư vấn|hỏi|không biết)\b");

        private bool IsAvailable(string m) =>
            Regex.IsMatch(m, @"\b(còn|trống|available|rảnh|free|đặt được)\b");

        private bool HasPriceKeyword(string m) =>
            Regex.IsMatch(m, @"\b(giá|tiền|budget|ngân sách|bao nhiêu|rẻ|đắt|triệu|nghìn|vnd|vnđ)\b");

        private bool HasCapacityKeyword(string m) =>
            Regex.IsMatch(m, @"\b(người|person|khách|guest|người lớn|\d+\s*(người|khách|pax))\b");

        private bool HasRoomTypeKeyword(string m) =>
            Regex.IsMatch(m, @"\b(vip|suite|deluxe|standard|superior|economy|loại|type|hạng)\b");

        private bool IsListAll(string m) =>
            Regex.IsMatch(m, @"\b(tất cả|danh sách|xem|show|list|all|hết)\b");

        // ══════════════════════════════════════════════════════════════
        //  HANDLERS
        // ══════════════════════════════════════════════════════════════

        private ChatResponse HandleGreeting() => new()
        {
            Message = "👋 **Xin chào! Tôi là trợ lý AI của Mường Thanh Hotel.**\n\n" +
                      "Tôi có thể giúp bạn:\n" +
                      "• 🔍 Tìm phòng theo **ngân sách** (ví dụ: *\"phòng dưới 2 triệu\"*)\n" +
                      "• 👥 Tìm phòng theo **số người** (ví dụ: *\"phòng cho 4 người\"*)\n" +
                      "• 🏷️ Tìm theo **loại phòng** (VIP, Deluxe, Standard...)\n" +
                      "• 📋 Xem **tất cả phòng** còn trống\n\n" +
                      "Bạn cần tìm phòng như thế nào?",
            SuggestedRooms = new()
        };

        private ChatResponse HandleHelp() => new()
        {
            Message = "💡 **Tôi có thể hỗ trợ bạn tìm phòng với các yêu cầu sau:**\n\n" +
                      "**Theo giá:**\n" +
                      "• *\"Phòng dưới 1 triệu\"* / *\"Budget 2 triệu\"*\n\n" +
                      "**Theo số người:**\n" +
                      "• *\"Phòng cho 2 người\"* / *\"4 khách\"*\n\n" +
                      "**Theo loại:**\n" +
                      "• *\"Phòng VIP\"* / *\"Phòng Deluxe\"*\n\n" +
                      "**Phòng trống:**\n" +
                      "• *\"Còn phòng nào trống không?\"*\n\n" +
                      "Hãy thử một câu hỏi! 😊",
            SuggestedRooms = new()
        };

        private ChatResponse HandleAvailable(List<Room> rooms)
        {
            var available = rooms.Where(r => r.Status == BusinessObjects.Enums.RoomStatus.Available).ToList();
            if (!available.Any())
                return new() { Message = "😔 Hiện tại **tất cả các phòng đều đã được đặt**. Vui lòng liên hệ lễ tân để biết lịch trống sớm nhất.", SuggestedRooms = new() };

            return new()
            {
                Message = $"✅ Hiện có **{available.Count} phòng đang trống** sẵn sàng đặt. Dưới đây là các phòng bạn có thể chọn:",
                SuggestedRooms = ToSuggested(available.Take(6).ToList(), "Phòng đang trống, sẵn sàng nhận đặt")
            };
        }

        private ChatResponse HandlePriceQuery(string msg, List<Room> rooms)
        {
            // Trích xuất số tiền từ message (triệu, nghìn, raw number)
            decimal? maxPrice = ExtractMaxPrice(msg);
            decimal? minPrice = ExtractMinPrice(msg);

            var filtered = rooms.Where(r =>
            {
                var price = r.RoomType?.PricePerNight ?? 0;
                if (maxPrice.HasValue && price > maxPrice.Value) return false;
                if (minPrice.HasValue && price < minPrice.Value) return false;
                return true;
            }).OrderBy(r => r.RoomType?.PricePerNight ?? 0).ToList();

            if (!filtered.Any())
            {
                string rangeText = maxPrice.HasValue ? $"dưới {maxPrice.Value:N0}₫" : "trong khoảng bạn yêu cầu";
                return new() { Message = $"😔 Không tìm thấy phòng **{rangeText}**. Bạn có thể tăng ngân sách hoặc xem tất cả phòng không?", SuggestedRooms = new() };
            }

            string priceDesc = maxPrice.HasValue ? $"dưới {maxPrice.Value:N0}₫/đêm" : "theo yêu cầu giá của bạn";
            return new()
            {
                Message = $"💰 Tìm thấy **{filtered.Count} phòng** {priceDesc}. Đây là các lựa chọn phù hợp nhất:",
                SuggestedRooms = ToSuggested(filtered.Take(5).ToList(), "Giá phù hợp với ngân sách của bạn")
            };
        }

        private ChatResponse HandleCapacityQuery(string msg, List<Room> rooms)
        {
            int? capacity = ExtractNumber(msg);
            if (!capacity.HasValue)
                return HandleSmartSearch(msg, rooms);

            var filtered = rooms
                .Where(r => r.MaxOccupancy >= capacity.Value)
                .OrderBy(r => r.MaxOccupancy)
                .ThenBy(r => r.RoomType?.PricePerNight ?? 0)
                .ToList();

            if (!filtered.Any())
                return new() { Message = $"😔 Không tìm thấy phòng nào chứa được **{capacity.Value} người**. Phòng lớn nhất của chúng tôi có sức chứa {rooms.Max(r => r.MaxOccupancy)} người.", SuggestedRooms = new() };

            return new()
            {
                Message = $"👥 Tìm thấy **{filtered.Count} phòng** phù hợp cho **{capacity.Value} người**. Đây là các gợi ý tốt nhất:",
                SuggestedRooms = ToSuggested(filtered.Take(5).ToList(), $"Sức chứa phù hợp cho {capacity.Value} người")
            };
        }

        private ChatResponse HandleRoomTypeQuery(string msg, List<Room> rooms)
        {
            var keywords = new[] { "vip", "suite", "deluxe", "standard", "superior", "economy", "executive" };
            string? matchedType = keywords.FirstOrDefault(k => msg.Contains(k));

            List<Room> filtered;
            string typeLabel;

            if (matchedType != null)
            {
                filtered = rooms
                    .Where(r => r.RoomType?.Name?.ToLower().Contains(matchedType) == true)
                    .ToList();
                typeLabel = matchedType.ToUpper();
            }
            else
            {
                // Không rõ loại cụ thể, lấy phòng cao cấp nhất
                filtered = rooms.OrderByDescending(r => r.RoomType?.PricePerNight ?? 0).Take(5).ToList();
                typeLabel = "cao cấp";
            }

            if (!filtered.Any())
                return new() { Message = $"😔 Không tìm thấy phòng loại **{typeLabel}**. Bạn muốn xem các loại phòng khác không?", SuggestedRooms = new() };

            return new()
            {
                Message = $"🏨 Tìm thấy **{filtered.Count} phòng** loại **{typeLabel}**:",
                SuggestedRooms = ToSuggested(filtered.Take(5).ToList(), $"Phòng loại {typeLabel}")
            };
        }

        private ChatResponse HandleAllRooms(List<Room> rooms)
        {
            if (!rooms.Any())
                return new() { Message = "📋 Hiện chưa có phòng nào trong hệ thống. Vui lòng liên hệ lễ tân.", SuggestedRooms = new() };

            var available = rooms.Where(r => r.Status == BusinessObjects.Enums.RoomStatus.Available).ToList();
            return new()
            {
                Message = $"📋 **Tổng {rooms.Count} phòng** trong hệ thống, trong đó **{available.Count} phòng đang trống**. Dưới đây là danh sách các phòng:",
                SuggestedRooms = ToSuggested(rooms.Take(6).ToList(), "")
            };
        }

        private ChatResponse HandleSmartSearch(string msg, List<Room> rooms)
        {
            // Kết hợp nhiều tiêu chí
            var scored = new List<(Room room, int score, string reason)>();

            int? capacity = ExtractNumber(msg);
            decimal? maxPrice = ExtractMaxPrice(msg);

            foreach (var room in rooms)
            {
                int score = 0;
                var reasons = new List<string>();

                if (capacity.HasValue && room.MaxOccupancy >= capacity.Value)
                { score += 3; reasons.Add($"sức chứa {room.MaxOccupancy} người"); }

                if (maxPrice.HasValue && (room.RoomType?.PricePerNight ?? 0) <= maxPrice.Value)
                { score += 3; reasons.Add($"giá {room.RoomType?.PricePerNight:N0}₫"); }

                if (room.Status == BusinessObjects.Enums.RoomStatus.Available)
                { score += 2; reasons.Add("còn trống"); }

                // Keyword match trong description/name
                var words = msg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var w in words.Where(w => w.Length > 2))
                {
                    if (room.RoomType?.Name?.ToLower().Contains(w) == true ||
                        room.Description?.ToLower().Contains(w) == true)
                    { score += 1; reasons.Add("phù hợp yêu cầu"); break; }
                }

                if (score > 0)
                    scored.Add((room, score, string.Join(", ", reasons.Distinct())));
            }

            if (scored.Any())
            {
                var top = scored.OrderByDescending(x => x.score).Take(4).ToList();
                return new()
                {
                    Message = "🔍 Dựa trên yêu cầu của bạn, đây là các phòng phù hợp nhất:",
                    SuggestedRooms = top.Select(x => new SuggestedRoom
                    {
                        Id = x.room.Id,
                        RoomNumber = x.room.RoomNumber,
                        RoomType = x.room.RoomType?.Name ?? "Phòng",
                        PricePerNight = x.room.RoomType?.PricePerNight ?? 0,
                        MaxOccupancy = x.room.MaxOccupancy,
                        Reason = string.IsNullOrEmpty(x.reason) ? "Phòng phù hợp" : x.reason,
                        ImageUrl = x.room.ImageUrl
                    }).ToList()
                };
            }

            // Fallback: hỏi lại
            return new()
            {
                Message = "🤔 Tôi chưa hiểu rõ yêu cầu của bạn. Bạn có thể thử:\n\n" +
                          "• *\"Phòng cho 2 người dưới 1 triệu\"*\n" +
                          "• *\"Phòng VIP còn trống\"*\n" +
                          "• *\"Xem tất cả phòng\"*",
                SuggestedRooms = new()
            };
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════

        private List<SuggestedRoom> ToSuggested(List<Room> rooms, string defaultReason) =>
            rooms.Select(r => new SuggestedRoom
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType?.Name ?? "Phòng",
                PricePerNight = r.RoomType?.PricePerNight ?? 0,
                MaxOccupancy = r.MaxOccupancy,
                Reason = defaultReason,
                ImageUrl = r.ImageUrl
            }).ToList();

        /// <summary>Trích xuất số tiền tối đa (max budget)</summary>
        private decimal? ExtractMaxPrice(string msg)
        {
            // Ví dụ: "dưới 2 triệu", "under 1.5 million", "2000000", "500k"
            var patterns = new[]
            {
                (@"(dưới|under|max|tối đa|không quá|<)\s*(\d+[\.,]?\d*)\s*(triệu|tr|million|m\b)", 1_000_000m),
                (@"(dưới|under|max|tối đa|không quá|<)\s*(\d+[\.,]?\d*)\s*(nghìn|k|ngàn|thousand)", 1_000m),
                (@"(dưới|under|max|tối đa|không quá|<)\s*(\d{4,})", 1m),
                (@"(\d+[\.,]?\d*)\s*(triệu|tr|million)\s*(trở xuống|trở lại|or less)?", 1_000_000m),
            };

            foreach (var (pattern, multiplier) in patterns)
            {
                var m = Regex.Match(msg, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    // Lấy group chứa số
                    for (int i = m.Groups.Count - 1; i >= 1; i--)
                    {
                        var val = m.Groups[i].Value.Replace(",", ".").Trim();
                        if (decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var num) && num > 0)
                            return num * multiplier;
                    }
                }
            }
            return null;
        }

        private decimal? ExtractMinPrice(string msg)
        {
            var m = Regex.Match(msg, @"(trên|from|từ|hơn|above|>)\s*(\d+[\.,]?\d*)\s*(triệu|tr|million)?", RegexOptions.IgnoreCase);
            if (m.Success && decimal.TryParse(m.Groups[2].Value.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var num))
            {
                bool hasMillion = m.Groups[3].Value.Length > 0;
                return hasMillion ? num * 1_000_000m : (num < 1000 ? num * 1_000_000m : num);
            }
            return null;
        }

        /// <summary>Trích xuất số nguyên đầu tiên trong message (số người)</summary>
        private int? ExtractNumber(string msg)
        {
            // Ưu tiên "X người/khách/person"
            var m = Regex.Match(msg, @"(\d+)\s*(người|khách|person|pax|guests?)", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n)) return n;

            // Fallback: số độc lập
            m = Regex.Match(msg, @"\b([1-9]\d?)\b");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n2) && n2 <= 20) return n2;

            return null;
        }
    }
}
