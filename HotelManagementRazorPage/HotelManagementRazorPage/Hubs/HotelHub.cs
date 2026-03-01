using Microsoft.AspNetCore.SignalR;

namespace HotelManagementRazorPage.Hubs
{
    public class HotelHub : Hub
    {
        // Nhận event từ client (không bắt buộc vì ta thường Invoke từ Controller/PageModel)
        public async Task SendRoomUpdate(int roomId, string status)
        {
            await Clients.All.SendAsync("ReceiveRoomUpdate", roomId, status);
        }
    }
}
