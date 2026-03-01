using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Staff
{
    [Authorize(Roles = "Staff,Manager,Admin")]
    public class BookingsModel : PageModel
    {
        private readonly IBookingService _bookingService;

        public BookingsModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> Bookings { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? FilterDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PhoneNumber { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public BookingStatus? Status { get; set; }

        public void OnGet()
        {
            Bookings = _bookingService.GetFilteredBookings(FilterDate, Status, PhoneNumber);
        }

        public IActionResult OnPostUpdateStatus(int id, BookingStatus newStatus)
        {
            try
            {
                _bookingService.UpdateStatus(id, newStatus);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }
    }
}
