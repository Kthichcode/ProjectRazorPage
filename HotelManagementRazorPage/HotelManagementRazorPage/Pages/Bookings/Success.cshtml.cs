using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.Security.Claims;

namespace HotelManagementRazorPage.Pages.Bookings
{
    [Authorize]
    public class SuccessModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public SuccessModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public Booking Booking { get; set; }

        public IActionResult OnGet(int id)
        {
            Booking = _bookingService.GetById(id);
            if (Booking == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Booking.CustomerId != userId) return Forbid();

            return Page();
        }
    }
}
