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
        private readonly IReviewService _reviewService;

        public HistoryModel(IBookingService bookingService, IVnPayService vnPayService,
                            IWalletService walletService, IReviewService reviewService)
        {
            _bookingService = bookingService;
            _vnPayService   = vnPayService;
            _walletService  = walletService;
            _reviewService  = reviewService;
        }

        public List<Booking> Bookings { get; set; } = new();
        public decimal WalletBalance { get; set; }

        [BindProperty(SupportsGet = true)]
        public BookingStatus? FilterStatus { get; set; }

        /// bookingId -> roomId (first room of booking that can still be reviewed)
        public Dictionary<int, int> ReviewableBookings { get; set; } = new();

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allBookings = _bookingService.GetMyBookings(userId);

            Bookings = (FilterStatus.HasValue
                ? allBookings.Where(b => b.Status == FilterStatus.Value)
                : allBookings)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            WalletBalance = _walletService.GetUserWallet(userId!).Balance;

            // For completed bookings, check if user can still leave a review
            foreach (var b in Bookings.Where(b => b.Status == BookingStatus.Completed))
            {
                var roomId = b.BookingRooms.FirstOrDefault()?.RoomId ?? 0;
                if (roomId > 0 && _reviewService.CanUserReview(userId!, roomId, out _))
                {
                    ReviewableBookings[b.Id] = roomId;
                }
            }
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
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
