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
    public class BookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IRoomService _roomService;
        private readonly IHubContext<HotelHub> _hubContext;

        public BookingsModel(IBookingService bookingService, IRoomService roomService, IHubContext<HotelHub> hubContext)
        {
            _bookingService = bookingService;
            _roomService = roomService;
            _hubContext = hubContext;
        }

        public List<Booking> Bookings { get; set; } = new();

        public void OnGet(int? roomId = null)
        {
            // Lấy các Booking đang chờ Check-in (Confirmed)
            // Lọc theo roomId nếu có truyền vào từ Dashboard
            Bookings = _bookingService.GetFilteredBookings(null, BookingStatus.Confirmed, "", roomId);
        }

        public async Task<IActionResult> OnPostCheckInAsync(int bookingId, int roomId)
        {
            if (roomId == 0)
            {
                TempData["Message"] = "Lỗi: Không tìm thấy phòng trong Booking này.";
                return RedirectToPage();
            }

            // 1. Cập nhật booking sang CheckedIn (Service sẽ tự đổi Room sang Occupied)
            _bookingService.UpdateStatus(bookingId, BookingStatus.CheckedIn);

            // 2. Gửi thông báo real-time qua Hub SignalR để màn hình RoomDashboard tự cập nhật màu
            await _hubContext.Clients.All.SendAsync("ReceiveRoomUpdate", roomId, "Occupied");

            TempData["Message"] = $"Đã Check-In thành công cho Booking #{bookingId}.";
            
            // Chuyển về trang Sơ đồ phòng để Staff thấy
            return RedirectToPage("/Staff/RoomDashboard");
        }
    }
}
