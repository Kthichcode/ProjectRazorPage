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

        public BookingService(IBookingRepository bookingRepo, IRoomRepository roomRepo, IPaymentRepository paymentRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _paymentRepo = paymentRepo;
        }

        public int CreateBooking(string userId, int roomId, DateTime checkIn, DateTime checkOut)
        {
            // 1. Validate info
            if (checkIn >= checkOut) throw new Exception("Check-out date must be after check-in date.");
            if (checkIn.Date < DateTime.Today) throw new Exception("Cannot book in the past.");

            // 2. Double-check availability logic
            // (Re-using logic from RoomService/Repository logic or checking directly)
            // Lấy lại danh sách phòng trống để chắc chắn phòng này chưa bị ai đặt trong lúc user đang suy nghĩ
            // Tuy nhiên để tối ưu, ta nên query trực tiếp vào DB check overlap cho RoomId cụ thể
            var room = _roomRepo.GetById(roomId);
            if (room == null) throw new Exception("Room not found.");
            if (room.Status != RoomStatus.Available) throw new Exception("Room is not available.");

            // Check overlap
            // Logic: Existing booking (CheckIn < newCheckout && CheckOut > newCheckIn)
            // Cần load bookings của room đó. 
            // Ở đây ta dùng GetAllWithBookings hoặc viết query mới. 
            // Để đơn giản và tận dụng repo hiện có, ta load room full
            // (Lưu ý: Nếu system lớn, nên viết query riêng CheckAvailability(roomId, start, end) trong Repo)
            
            // Tạm thời dùng logic client-side loading (chấp nhận performance nhẹ bước này vì project nhỏ)
            // room đã include BookingRooms rồi (do GetById của RoomRepo có Include)
            
            // Reload room để lấy full booking (vì GetById có thể chỉ lấy basic) 
            // Check RoomRepo: GetById có Include BookingRooms. Tốt.
            
            foreach (var br in room.BookingRooms)
            {
                var b = br.Booking;
                if (b == null) continue;
                if (b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Completed) continue; // Completed vẫn tính là đã ở xong, ko ảnh hưởng tương lai? 
                // Ah, booking cũ completed thì ko sao. Booking Confirmed/Pending/CheckedIn mới lo.
                // Thực tế: Nếu lịch cũ đã completed thì thời gian của nó phải < checkIn mới.
                // Logic overlap: (StartA < EndB) && (EndA > StartB)
                
                if (checkIn < b.CheckOutDate && checkOut > b.CheckInDate)
                {
                    throw new Exception("This room is already booked for the selected dates.");
                }
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
            if (booking == null) throw new Exception("Booking not found.");

            // Security check
            if (booking.CustomerId != userId) throw new Exception("You are not authorized to cancel this booking.");

            // Logic check
            if (booking.Status == BookingStatus.Cancelled) throw new Exception("Booking is already cancelled.");
            if (booking.Status == BookingStatus.Completed) throw new Exception("Cannot cancel completed booking.");
            
            // Rule: Cancel before check-in
            // Rule: Cancel before check-in date
            // Relaxed rule: Allow cancellation even on the check-in day as long as they haven't checked in (Status check handles that)
            // Stricter rule would be: if (booking.CheckInDate < DateTime.Today)
            
            if (booking.CheckInDate < DateTime.Today) throw new Exception("Cannot cancel past bookings.");

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
                     throw new InvalidOperationException("Cannot confirm payment for a Cancelled booking. Please contact support for refund.");
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
        public List<Booking> GetFilteredBookings(DateTime? date, BookingStatus? status, string phoneNumber)
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

            return query.OrderByDescending(b => b.CreatedAt).ToList();
        }

        public void UpdateStatus(int bookingId, BookingStatus newStatus)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Booking not found.");

            // Validation Logic for Status Transitions
            // Flow: Pending -> Confirmed -> CheckedIn -> Completed
            //               -> Cancelled (Anytime before CheckIn)

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                throw new Exception($"Cannot change status of a {booking.Status} booking.");
            }

            if (newStatus == BookingStatus.Confirmed)
            {
                if (booking.Status != BookingStatus.Pending) throw new Exception("Only Pending bookings can be Confirmed.");
            }
            else if (newStatus == BookingStatus.CheckedIn)
            {
                if (booking.Status != BookingStatus.Confirmed) throw new Exception("Only Confirmed bookings can be Checked In.");
            }
            else if (newStatus == BookingStatus.Completed)
            {
                if (booking.Status != BookingStatus.CheckedIn) throw new Exception("Only Checked In bookings can be Completed.");
            }
            else if (newStatus == BookingStatus.Cancelled)
            {
                // Can cancel Pending or Confirmed
                if (booking.Status == BookingStatus.CheckedIn || booking.Status == BookingStatus.Completed)
                    throw new Exception("Cannot cancel after checking in.");
            }

            _bookingRepo.UpdateStatus(bookingId, newStatus);
            _bookingRepo.Save();
        }

        public List<Booking> SearchBookingsByPhoneNumber(string phoneNumber)
        {
            return _bookingRepo.GetByCustomerPhoneNumber(phoneNumber);
        }

    }
}
