using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Manager.Bookings
{
    [Authorize(Roles = "Manager,Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public DetailsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public Booking? Booking { get; set; }

        public IActionResult OnGet(int id)
        {
            Booking = _bookingService.GetById(id);
            if (Booking == null) return NotFound();
            return Page();
        }

        public IActionResult OnPostUpdateStatus(int bookingId, int newStatus)
        {
            try
            {
                var status = (BookingStatus)newStatus;
                _bookingService.UpdateStatus(bookingId, status);

                string msg = status switch
                {
                    BookingStatus.Confirmed => "Đã xác nhận đặt phòng thành công.",
                    BookingStatus.CheckedIn => "Khách đã Check-In thành công.",
                    BookingStatus.Completed => "Booking đã hoàn thành (Check-Out).",
                    BookingStatus.Cancelled => "Đã hủy đặt phòng.",
                    _ => "Cập nhật trạng thái thành công."
                };
                TempData["Success"] = msg;
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToPage(new { id = bookingId });
        }

        public IActionResult OnPostApproveCancel(int bookingId)
        {
            try
            {
                _bookingService.ApproveCancel(bookingId);
                TempData["Success"] = "Đã duyệt hủy và hoàn tiền vào ví khách hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToPage(new { id = bookingId });
        }
    }
}
