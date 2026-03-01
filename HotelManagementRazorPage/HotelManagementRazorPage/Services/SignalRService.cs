using HotelManagementRazorPage.Hubs;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace HotelManagementRazorPage.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<RoomHub> _hubContext;

        public SignalRService(IHubContext<RoomHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendRoomStatusUpdate(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", message);
        }
    }
}
