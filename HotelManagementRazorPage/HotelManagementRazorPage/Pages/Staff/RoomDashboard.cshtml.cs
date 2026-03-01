using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Staff
{
    [Authorize(Roles = "Staff")]
    public class RoomDashboardModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;

        public RoomDashboardModel(IRoomService roomService, IBookingService bookingService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
        }

        public List<Room> Rooms { get; set; } = new();

        public void OnGet()
        {
            Rooms = _roomService.GetAllWithBookings();
            Rooms = Rooms.OrderBy(r => r.RoomNumber).ToList();
        }

        public IActionResult OnPostCheckOut(int bookingId)
        {
            try
            {
                _bookingService.UpdateStatus(bookingId, BookingStatus.Completed);
                TempData["SuccessMessage"] = "Trả phòng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostCheckIn(int bookingId)
        {
            try
            {
                _bookingService.UpdateStatus(bookingId, BookingStatus.CheckedIn);
                TempData["SuccessMessage"] = "Check-in thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public JsonResult OnGetBookingsByRoom(int roomId)
        {
            // Lấy tất cả booking của phòng này mà đang ở trạng thái Confirmed (Chờ nhận phòng)
            var room = _roomService.GetAllWithBookings().FirstOrDefault(r => r.Id == roomId);
            if (room == null) return new JsonResult(new List<object>());

            var pendingBookings = room.BookingRooms
                .Select(br => br.Booking)
                .Where(b => b != null && b.Status == BookingStatus.Confirmed)
                .Select(b => new {
                    id = b.Id,
                    customerName = b.Customer?.FullName ?? b.Customer?.UserName ?? "---",
                    phoneNumber = b.Customer?.PhoneNumber ?? "---",
                    checkIn = b.CheckInDate.ToString("dd/MM/yyyy"),
                    checkOut = b.CheckOutDate.ToString("dd/MM/yyyy")
                })
                .ToList();

            return new JsonResult(pendingBookings);
        }

        public Booking? GetActiveBooking(int roomId)
        {
            // Lay booking dang CheckedIn hoac Confirmed ma co ngay hien tai nam trong khoang CheckIn/CheckOut
            var bookings = _bookingService.GetFilteredBookings(DateTime.Today, null, "", roomId);
            // Ưu tiên lấy booking đang CheckedIn, nếu không có thì lấy Confirmed cho ngày hôm nay
            return bookings.FirstOrDefault(b => b.Status == BookingStatus.CheckedIn) 
                   ?? bookings.FirstOrDefault(b => b.Status == BookingStatus.Confirmed && b.CheckInDate.Date <= DateTime.Today);
        }
    }
}
