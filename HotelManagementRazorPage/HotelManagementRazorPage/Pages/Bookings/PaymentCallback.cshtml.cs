using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Bookings
{
    public class PaymentCallbackModel : PageModel
    {
        private readonly IVnPayService _vnPayService;
        private readonly IBookingService _bookingService;

        public PaymentCallbackModel(IVnPayService vnPayService, IBookingService bookingService)
        {
            _vnPayService = vnPayService;
            _bookingService = bookingService;
        }

        public string Message { get; set; } = "Đang xử lý thanh toán...";
        public bool IsSuccess { get; set; } = false;
        public int? BookingId { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var collections = Request.Query;
                if (!collections.Any())
                {
                    Message = "Không có thông tin trả về từ VNPAY.";
                    return Page();
                }

                var response = _vnPayService.PaymentExecute(collections);

                if (response == null)
                {
                    Message = "Lỗi trong quá trình xử lý giao dịch.";
                    return Page();
                }

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    IsSuccess = true;
                    if (int.TryParse(response.OrderId, out int bId))
                    {
                        BookingId = bId;
                        // Xác nhận thanh toán thành công
                        // bookingService.ConfirmPayment đã tạo record Payment và update status Booking thành Confirmed
                        // (Lưu ý: Nếu user click F5 thì có thể văng lỗi Cannot confirm payment for a Cancelled.. 
                        // -> Nên ktra lại trạng thái trước khi update để tránh lỗi)
                        
                        var booking = _bookingService.GetById(bId);
                        if (booking != null && booking.Status == BookingStatus.Pending)
                        {
                            _bookingService.ConfirmPayment(bId, response.TransactionId ?? "");
                        }
                    }

                    Message = "Thanh toán thành công qua VNPAY.";
                    // Có thể tự động chuyển sang trang Success
                    if (BookingId.HasValue)
                    {
                        return RedirectToPage("Success", new { id = BookingId.Value });
                    }
                }
                else
                {
                    Message = $"Giao dịch thất bại: Lỗi {response.VnPayResponseCode}";
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi hệ thống: {ex.Message}";
                IsSuccess = false;
            }

            return Page();
        }
    }
}
