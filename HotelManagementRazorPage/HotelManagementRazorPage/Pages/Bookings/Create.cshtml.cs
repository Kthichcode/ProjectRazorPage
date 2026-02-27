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
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IBookingService _bookingService;
        private readonly IVnPayService _vnPayService;

        public CreateModel(IRoomService roomService, IBookingService bookingService, IVnPayService vnPayService)
        {
            _roomService = roomService;
            _bookingService = bookingService;
            _vnPayService = vnPayService;
        }

        public Room Room { get; set; }

        [BindProperty]
        public DateTime CheckIn { get; set; } = DateTime.Today;

        [BindProperty]
        public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);

        [BindProperty]
        public string PaymentMethod { get; set; } = "Cash";

        public string ErrorMessage { get; set; }

        public IActionResult OnGet(int roomId)
        {
            Room = _roomService.GetById(roomId);
            if (Room == null || Room.Status != RoomStatus.Available)
            {
                return RedirectToPage("/Rooms/Index");
            }
            return Page();
        }

        public IActionResult OnPost(int roomId)
        {
            Room = _roomService.GetById(roomId);
            if (Room == null) return NotFound();

            if (CheckIn >= CheckOut)
            {
                ErrorMessage = "Ngày trả phòng phải sau ngày nhận phòng.";
                return Page();
            }

            if (CheckIn.Date < DateTime.Today)
            {
                ErrorMessage = "Không thể đặt phòng trong quá khứ.";
                return Page();
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // 1. Tạo Booking
                int bookingId = _bookingService.CreateBooking(userId, roomId, CheckIn, CheckOut);
                
                // Load lại Booking để lấy thông tin TotalAmount
                var booking = _bookingService.GetById(bookingId);

                // 2. Chuyển hướng thanh toán
                if (PaymentMethod == "VNPay")
                {
                    var paymentUrl = _vnPayService.CreatePaymentUrl(booking, HttpContext, booking.TotalAmount);
                    return Redirect(paymentUrl);
                }

                // Tiền mặt thì sang Success luôn
                return RedirectToPage("Success", new { id = bookingId });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
