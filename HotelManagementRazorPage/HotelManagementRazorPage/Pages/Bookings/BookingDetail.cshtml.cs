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
    public class BookingDetailModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly IReviewService _reviewService;

        public BookingDetailModel(IBookingService bookingService, IReviewService reviewService)
        {
            _bookingService = bookingService;
            _reviewService  = reviewService;
        }

        public Booking Booking { get; set; } = null!;
        public bool CanReview { get; set; }
        public int ReviewableRoomId { get; set; }

        public IActionResult OnGet(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = _bookingService.GetById(id);

            if (booking == null || booking.CustomerId != userId)
                return RedirectToPage("/Bookings/History");

            Booking = booking;

            // Check if user can review
            if (booking.Status == BookingStatus.Completed)
            {
                var roomId = booking.BookingRooms.FirstOrDefault()?.RoomId ?? 0;
                if (roomId > 0 && _reviewService.CanUserReview(userId!, roomId, out _))
                {
                    CanReview = true;
                    ReviewableRoomId = roomId;
                }
            }

            return Page();
        }
    }
}
