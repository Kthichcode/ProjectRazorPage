using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.Text.Json;

namespace HotelManagementRazorPage.Pages.Chat
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IAiChatService _aiChatService;

        public IndexModel(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostSendAsync([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return new JsonResult(new { message = "Vui lòng nhập tin nhắn.", suggestedRooms = new List<object>() });
            }

            var history = request.History ?? new List<ChatMessage>();
            var result = await _aiChatService.SendMessageAsync(request.Message, history);

            return new JsonResult(new
            {
                message = result.Message,
                suggestedRooms = result.SuggestedRooms.Select(r => new
                {
                    id = r.Id,
                    roomNumber = r.RoomNumber,
                    roomType = r.RoomType,
                    pricePerNight = r.PricePerNight,
                    maxOccupancy = r.MaxOccupancy,
                    reason = r.Reason,
                    imageUrl = r.ImageUrl
                })
            });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
        public List<ChatMessage>? History { get; set; }
    }
}
