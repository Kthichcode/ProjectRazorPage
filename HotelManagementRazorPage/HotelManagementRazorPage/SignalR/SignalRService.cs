using HotelManagementRazorPage.Hubs;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace HotelManagementRazorPage.SignalR
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

        public async Task SendNewBooking(int bookingId, string customerName, string phoneNumber, string roomNumbers, decimal totalAmount, DateTime checkIn, DateTime checkOut)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceiveNewBooking",
                bookingId,
                customerName,
                phoneNumber,
                roomNumbers,
                (long)totalAmount,
                checkIn.ToString("dd/MM/yyyy"),
                checkOut.ToString("dd/MM/yyyy"),
                DateTime.Now.ToString("HH:mm dd/MM")
            );
        }
        public async Task SendRoomUpdate(string action, string roomNumber)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveRoomUpdate", action, roomNumber);
        }

        public async Task SendPaymentSuccess(int bookingId, string customerName, decimal amount)
        {
            await _hubContext.Clients.All.SendAsync(
                "ReceivePaymentSuccess",
                bookingId,
                customerName,
                (long)amount,
                DateTime.Now.ToString("HH:mm dd/MM/yyyy")
            );
        }
    }
}
