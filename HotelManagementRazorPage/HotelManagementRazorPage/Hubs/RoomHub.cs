using Microsoft.AspNetCore.SignalR;

namespace HotelManagementRazorPage.Hubs
{
    public class RoomHub : Hub
    {
        public async Task SendStatusUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", message);
        }
    }
}
