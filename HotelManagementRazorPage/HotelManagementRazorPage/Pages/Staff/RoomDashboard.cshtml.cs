using BusinessObjects.Entities;
using BusinessObjects.Enums;
using HotelManagementRazorPage.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Staff
{
    [Authorize(Roles = "Staff")]
    public class RoomDashboardModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly IHubContext<HotelHub> _hubContext;

        public RoomDashboardModel(IRoomService roomService, IBookingService bookingService, IHubContext<HotelHub> hubContext)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _hubContext = hubContext;
        }

        public List<Room> Rooms { get; set; } = new List<Room>();

        public void OnGet()
        {
            Rooms = _roomService.GetAllWithBookings();
        }

        public async Task<IActionResult> OnPostCheckOutAsync(int bookingId, int roomId)
        {
            // 1. Cập nhật booking sang Completed (Service sẽ tự đổi Room sang Available)
            _bookingService.UpdateStatus(bookingId, BookingStatus.Completed);

            // 2. Gửi thông báo real-time
            await _hubContext.Clients.All.SendAsync("ReceiveRoomUpdate", roomId, "Available");

            // Them alert (optional)
            TempData["SuccessMessage"] = $"Đã Check-Out thành công phòng có ID {roomId}";
            
            return RedirectToPage();
        }
    }
}
