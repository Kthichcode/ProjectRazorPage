using BusinessObjects.Entities;
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

        public BookingDetailModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public Booking Booking { get; set; } = null!;

        public IActionResult OnGet(int id)
        {
            var booking = _bookingService.GetById(id);
            if (booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (booking.CustomerId != userId) return Forbid();

            Booking = booking;
            return Page();
        }
    }
}
