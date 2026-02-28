using System.Threading.Tasks;

namespace Services.Interfaces
{
    public class ChatMessage
    {
        public string Role { get; set; } = ""; // "user" or "assistant"
        public string Content { get; set; } = "";
    }

    public class ChatResponse
    {
        public string Message { get; set; } = "";
        public List<SuggestedRoom> SuggestedRooms { get; set; } = new();
    }

    public class SuggestedRoom
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = "";
        public string RoomType { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public int MaxOccupancy { get; set; }
        public string Reason { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }

    public interface IAiChatService
    {
        Task<ChatResponse> SendMessageAsync(string userMessage, List<ChatMessage> history);
    }
}
