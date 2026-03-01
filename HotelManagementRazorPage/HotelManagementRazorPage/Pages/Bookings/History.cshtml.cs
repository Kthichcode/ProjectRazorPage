using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.Security.Claims;

namespace HotelManagementRazorPage.Pages.Bookings
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IVnPayService _vnPayService;
        private readonly IWalletService _walletService;

        public HistoryModel(IBookingService bookingService, IVnPayService vnPayService, IWalletService walletService)
        {
            _bookingService = bookingService;
            _vnPayService = vnPayService;
            _walletService = walletService;
        }

        public List<Booking> Bookings { get; set; } = new();
        public decimal WalletBalance { get; set; }

        [BindProperty(SupportsGet = true)]
        public BookingStatus? FilterStatus { get; set; }

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allBookings = _bookingService.GetMyBookings(userId);

            if (FilterStatus.HasValue)
            {
                Bookings = allBookings.Where(b => b.Status == FilterStatus.Value).ToList();
            }
            else
            {
                Bookings = allBookings;
            }

            Bookings = Bookings.OrderByDescending(b => b.CreatedAt).ToList();
            WalletBalance = _walletService.GetUserWallet(userId!).Balance;
        }

        public IActionResult OnPostCancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _bookingService.RequestCancellation(id, userId!);
                TempData["SuccessMessage"] = "Đã gửi yêu cầu hủy. Vui lòng chờ manager duyệt để được hoàn tiền vào ví.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostRetryPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = _bookingService.GetById(id);

            if (booking == null || booking.CustomerId != userId || booking.Status != BookingStatus.Pending)
            {
                TempData["ErrorMessage"] = "Không thể thanh toán đơn này.";
                return RedirectToPage();
            }

            var paymentUrl = _vnPayService.CreatePaymentUrl(booking, HttpContext, booking.TotalAmount);
            return Redirect(paymentUrl);
        }
    }
}
