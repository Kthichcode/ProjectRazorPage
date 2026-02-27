using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Admin.Bookings
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public IndexModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> Bookings { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public BookingStatus? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterPhone { get; set; } = "";

        public void OnGet()
        {
            Bookings = _bookingService.GetFilteredBookings(FilterDate, FilterStatus, FilterPhone);
        }
    }
}
