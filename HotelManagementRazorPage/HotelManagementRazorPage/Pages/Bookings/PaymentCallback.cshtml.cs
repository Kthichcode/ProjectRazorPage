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
        private readonly IWalletService _walletService;
        private readonly ISignalRService _signalRService;

        public PaymentCallbackModel(IVnPayService vnPayService, IBookingService bookingService, IWalletService walletService, ISignalRService signalRService)
        {
            _vnPayService = vnPayService;
            _bookingService = bookingService;
            _walletService = walletService;
            _signalRService = signalRService;
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

                        var booking = _bookingService.GetById(bId);
                        if (booking != null && booking.Status == BookingStatus.Pending)
                        {
                            // Đọc WalletAmountPaid từ DB (được lưu trước khi redirect sang VNPay)
                            decimal walletPaid = booking.WalletAmountPaid ?? 0;

                            if (walletPaid > 0)
                            {
                                // Hybrid: ví + VNPay
                                _bookingService.ConfirmPaymentWithWallet(bId, walletPaid, response.TransactionId ?? "");
                            }
                            else
                            {
                                // Thuần VNPay
                                _bookingService.ConfirmPayment(bId, response.TransactionId ?? "");
                            }
                        }
                    }

                    Message = "Thanh toán thành công qua VNPAY.";
                    if (BookingId.HasValue)
                    {
                        return RedirectToPage("Success", new { id = BookingId.Value });
                    }
                }
                else
                {
                    // VNPay thất bại → hoàn tiền ví nếu có hybrid payment
                    if (int.TryParse(response?.OrderId, out int failedBId))
                    {
                        var booking = _bookingService.GetById(failedBId);
                        if (booking != null)
                        {
                            decimal walletPaid = booking.WalletAmountPaid ?? 0;
                            if (walletPaid > 0)
                            {
                                // Hoàn lại tiền ví vì VNPay thất bại
                                _walletService.AddBalance(
                                    booking.CustomerId,
                                    walletPaid,
                                    $"Hoàn tiền ví do VNPay thất bại - booking #{failedBId}"
                                );
                                // Reset WalletAmountPaid
                                booking.WalletAmountPaid = null;
                                _bookingService.SaveBookingChanges(failedBId, booking);
                            }
                        }
                    }

                    Message = $"Giao dịch thất bại: Lỗi {response?.VnPayResponseCode}";
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
