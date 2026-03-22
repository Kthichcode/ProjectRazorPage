using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IWalletService _walletService;

        private readonly ISignalRService _signalRService;

        public BookingService(IBookingRepository bookingRepo, IRoomRepository roomRepo, IPaymentRepository paymentRepo, IWalletService walletService, ISignalRService signalRService)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _paymentRepo = paymentRepo;
            _walletService = walletService;
            _signalRService = signalRService;
        }

        public int CreateBooking(string userId, int roomId, DateTime checkIn, DateTime checkOut)
        {
            // 1. Validate info
            if (checkIn >= checkOut) throw new Exception("Ngày trả phòng phải sau ngày nhận phòng.");
            if (checkIn.Date < DateTime.Today) throw new Exception("Không thể đặt phòng cho ngày ở quá khứ.");

            // 2. Double-check availability logic
            // (Re-using logic from RoomService/Repository logic or checking directly)
            // Lấy lại danh sách phòng trống để chắc chắn phòng này chưa bị ai đặt trong lúc user đang suy nghĩ
            // Tuy nhiên để tối ưu, ta nên query trực tiếp vào DB check overlap cho RoomId cụ thể
            var room = _roomRepo.GetById(roomId);
            if (room == null) throw new Exception("Không tìm thấy phòng.");

            // 2. Check for conflicting bookings via direct DB query (navigation property is not loaded here)
            if (_bookingRepo.HasOverlapBooking(roomId, checkIn, checkOut))
            {
                throw new Exception("Phòng này đã được đặt trong thời gian bạn chọn.");
            }

            // 3. Calculate Total Amount
            int nights = (int)(checkOut - checkIn).TotalDays;
            if (nights < 1) nights = 1;
            decimal price = room.RoomType != null ? room.RoomType.PricePerNight : 0;
            decimal total = nights * price;

            // 4. Create Booking Entities
            var booking = new Booking
            {
                CustomerId = userId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalAmount = total,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var bookingRoom = new BookingRoom
            {
                Booking = booking,
                RoomId = roomId
            };
            
            booking.BookingRooms.Add(bookingRoom);

            _bookingRepo.Add(booking);
            _bookingRepo.Save();

            return booking.Id;
        }

        public List<Booking> GetMyBookings(string userId)
        {
            return _bookingRepo.GetByCustomer(userId);
        }

        public Booking? GetById(int id)
        {
            return _bookingRepo.GetById(id);
        }

        public void CancelBooking(int bookingId, string userId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy đơn đặt phòng.");

            // Security check
            if (booking.CustomerId != userId) throw new Exception("Bạn không có quyền hủy đơn đặt phòng này.");

            // Logic check
            if (booking.Status == BookingStatus.Cancelled) throw new Exception("Đơn đặt phòng này đã được hủy trước đó.");
            if (booking.Status == BookingStatus.Completed) throw new Exception("Không thể hủy đơn đặt phòng đã hoàn thành.");
            
            // Rule: Cancel before check-in
            // Rule: Cancel before check-in date
            // Relaxed rule: Allow cancellation even on the check-in day as long as they haven't checked in (Status check handles that)
            // Stricter rule would be: if (booking.CheckInDate < DateTime.Today)
            
            if (booking.CheckInDate < DateTime.Today) throw new Exception("Không thể hủy đơn đặt phòng trong quá khứ.");

            _bookingRepo.UpdateStatus(bookingId, BookingStatus.Cancelled);
            _bookingRepo.Save();
        }
        public void ConfirmPayment(int bookingId, string transactionId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if(booking != null)
            {
                // Prevent resurrecting Cancelled bookings
                if (booking.Status == BookingStatus.Cancelled)
                {
                     throw new InvalidOperationException("Không thể xác nhận thanh toán cho đơn đã hủy. Vui lòng liên hệ bộ phận hỗ trợ.");
                }

                // If already confirmed/paid, we might still want to record the transaction if it's new?
                // But generally ConfirmPayment implies moving state.
                // For now, let's proceed.

                // Update booking status
                _bookingRepo.UpdateStatus(bookingId, BookingStatus.Confirmed);
                
                // Create Payment Record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = booking.TotalAmount,
                    Method = "VNPay",
                    Status = PaymentStatus.Paid,
                    ProviderTransactionId = transactionId, // Store the ID
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow
                };

                _paymentRepo.Add(payment);
                _paymentRepo.Save();
                 _bookingRepo.Save();

                BroadcastConfirmedBooking(bookingId);
            }
        }
        public void RecordPayment(int bookingId, decimal amount, string method, string transactionId)
        {
             var payment = new Payment
             {
                 BookingId = bookingId,
                 Amount = amount,
                 Method = method,
                 Status = PaymentStatus.Paid,
                 ProviderTransactionId = transactionId,
                 CreatedAt = DateTime.UtcNow,
                 PaidAt = DateTime.UtcNow
             };
             _paymentRepo.Add(payment);
             _paymentRepo.Save();
        }

        public void ConfirmFullWalletPayment(int bookingId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");
            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Không thể xác nhận thanh toán cho đơn đã hủy.");

            _bookingRepo.UpdateStatus(bookingId, BookingStatus.Confirmed);

            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = booking.TotalAmount,
                Method = "Wallet",
                Status = PaymentStatus.Paid,
                ProviderTransactionId = "WALLET",
                CreatedAt = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };
            _paymentRepo.Add(payment);
            _paymentRepo.Save();
            _bookingRepo.Save();

            BroadcastConfirmedBooking(bookingId);
        }

        public void SaveBookingChanges(int bookingId, Booking updatedBooking)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");
            booking.WalletAmountPaid = updatedBooking.WalletAmountPaid;
            _bookingRepo.Save();
        }

        public void ConfirmPaymentWithWallet(int bookingId, decimal walletAmountPaid, string vnpayTransactionId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");
            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Không thể xác nhận thanh toán cho đơn đã hủy.");

            _bookingRepo.UpdateStatus(bookingId, BookingStatus.Confirmed);

            // Record wallet portion
            if (walletAmountPaid > 0)
            {
                var walletPayment = new Payment
                {
                    BookingId = bookingId,
                    Amount = walletAmountPaid,
                    Method = "Wallet",
                    Status = PaymentStatus.Paid,
                    ProviderTransactionId = "WALLET",
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow
                };
                _paymentRepo.Add(walletPayment);
            }

            // Record VNPay portion
            decimal vnpayAmount = booking.TotalAmount - walletAmountPaid;
            if (vnpayAmount > 0)
            {
                var vnpayPayment = new Payment
                {
                    BookingId = bookingId,
                    Amount = vnpayAmount,
                    Method = "VNPay",
                    Status = PaymentStatus.Paid,
                    ProviderTransactionId = vnpayTransactionId,
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow
                };
                _paymentRepo.Add(vnpayPayment);
            }

            _paymentRepo.Save();
            _bookingRepo.Save();

            BroadcastConfirmedBooking(bookingId);
        }

        private void BroadcastConfirmedBooking(int bookingId)
        {
            try
            {
                var b = _bookingRepo.GetById(bookingId);
                if (b == null) return;
                string customerName = b.Customer?.FullName ?? b.Customer?.UserName ?? "Khách hàng";
                string phoneNumber  = b.Customer?.PhoneNumber ?? "";
                string roomNums = b.BookingRooms.Any()
                    ? string.Join(", ", b.BookingRooms.Select(br => br.Room?.RoomNumber ?? "?"))
                    : "?";
                _signalRService.SendNewBooking(
                    b.Id, customerName, phoneNumber, roomNums, b.TotalAmount, b.CheckInDate, b.CheckOutDate
                ).Wait();

                // Gửi thông báo thanh toán thành công cụ thể cho staff
                _signalRService.SendPaymentSuccess(b.Id, customerName, b.TotalAmount).Wait();
            }
            catch { /* never break main flow */ }
        }

 public List<Booking> GetFilteredBookings(DateTime? date, BookingStatus? status, string phoneNumber, int? roomId = null)
        {
            var query = _bookingRepo.GetQuery();

            if (date.HasValue)
            {
                // Filter by CheckInDate matching the date
                query = query.Where(b => b.CheckInDate.Date == date.Value.Date);
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(b => b.Customer.PhoneNumber.Contains(phoneNumber));
            }

            if (roomId.HasValue)
            {
                query = query.Where(b => b.BookingRooms.Any(br => br.RoomId == roomId.Value));
            }

            return query.OrderByDescending(b => b.CreatedAt).ToList();
        }

        public void UpdateStatus(int bookingId, BookingStatus newStatus)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy đơn đặt phòng.");

            // Validation Logic for Status Transitions
            // Flow: Pending -> Confirmed -> CheckedIn -> Completed
            //               -> Cancelled (Anytime before CheckIn)

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                throw new Exception($"Cannot change status of a {booking.Status} booking.");
            }

            if (newStatus == BookingStatus.CheckedIn)
            {
                if (booking.Status != BookingStatus.Confirmed) throw new Exception("Chỉ có thể Check-in cho các đơn đã xác nhận.");
            }
            else if (newStatus == BookingStatus.Completed)
            {
                if (booking.Status != BookingStatus.CheckedIn) throw new Exception("Chỉ có thể hoàn thành các đơn đã Check-in.");
            }
            else if (newStatus == BookingStatus.Cancelled)
            {
                // Can cancel Pending or Confirmed
                if (booking.Status == BookingStatus.CheckedIn || booking.Status == BookingStatus.Completed)
                    throw new Exception("Không thể hủy đơn đặt phòng sau khi đã Check-in.");
            }

            _bookingRepo.UpdateStatus(bookingId, newStatus);
            
            // Sync Room Status
            if (newStatus == BookingStatus.CheckedIn)
            {
                foreach (var br in booking.BookingRooms)
                {
                    var room = _roomRepo.GetById(br.RoomId);
                    if (room != null)
                    {
                        room.Status = RoomStatus.Occupied;
                        _roomRepo.Update(room);
                    }
                }
            }
            else if (newStatus == BookingStatus.Completed || newStatus == BookingStatus.Cancelled)
            {
                foreach (var br in booking.BookingRooms)
                {
                    var room = _roomRepo.GetById(br.RoomId);
                    if (room != null)
                    {
                        room.Status = RoomStatus.Available;
                        _roomRepo.Update(room);
                    }
                }
            }

            _bookingRepo.Save();
            _roomRepo.Save();

            // Notify real-time
            _signalRService.SendRoomStatusUpdate($"Booking #{bookingId} updated to {newStatus}").Wait();
        }

        public List<Booking> SearchBookingsByPhoneNumber(string phoneNumber)
        {
            return _bookingRepo.GetByCustomerPhoneNumber(phoneNumber);
        }

        // ── Cancellation with Refund Flow ──────────────────────────────────────────

        public void RequestCancellation(int bookingId, string userId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");

            if (booking.CustomerId != userId)
                throw new Exception("Bạn không có quyền thực hiện thao tác này.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new Exception("Booking này đã bị hủy rồi.");

            if (booking.Status == BookingStatus.CancellationPending)
                throw new Exception("Yêu cầu hủy đã được gửi, đang chờ manager duyệt.");

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CheckedIn)
                throw new Exception("Không thể hủy booking đã check-in hoặc hoàn thành.");

            if (booking.CheckInDate.Date < DateTime.Today)
                throw new Exception("Không thể hủy booking trong quá khứ.");

            booking.CancellationRequestedAt = DateTime.UtcNow;
            booking.Status = BookingStatus.CancellationPending;
            _bookingRepo.Save();
        }

        public void ApproveCancel(int bookingId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Không tìm thấy booking.");

            if (booking.Status != BookingStatus.CancellationPending)
                throw new Exception("Booking này không ở trạng thái chờ duyệt hủy.");

            var requestedAt = booking.CancellationRequestedAt ?? DateTime.UtcNow;

            // Logic hoàn tiền dựa trên số ngày còn lại đến ngày nhận phòng
            var daysUntilCheckIn = (booking.CheckInDate.Date - requestedAt.Date).TotalDays;

            decimal refundPercent;
            string refundDesc;
            if (daysUntilCheckIn <= 3)
            {
                // Hủy trong vòng 1–3 ngày trước check-in → mất toàn bộ
                refundPercent = 0m;
                refundDesc = $"Hủy booking #{bookingId} (không hoàn tiền)";
            }
            else if (daysUntilCheckIn < 7)
            {
                // Hủy trước 4–6 ngày → hoàn 70%
                refundPercent = 0.7m;
                refundDesc = $"Hoàn 70% tiền booking #{bookingId}";
            }
            else
            {
                // Hủy trước từ 7 ngày trở lên → hoàn 100%
                refundPercent = 1.0m;
                refundDesc = $"Hoàn 100% tiền booking #{bookingId}";
            }

            decimal refundAmount = Math.Round(booking.TotalAmount * refundPercent, 0);

            booking.RefundAmount = refundAmount;
            booking.Status = BookingStatus.Cancelled;
            _bookingRepo.Save();

            // Cộng tiền vào ví khách (nếu có)
            if (refundAmount > 0)
                _walletService.AddBalance(booking.CustomerId, refundAmount, refundDesc);
        }

    }
}
