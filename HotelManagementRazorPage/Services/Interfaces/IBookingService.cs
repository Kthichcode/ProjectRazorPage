using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IBookingService
    {
        int CreateBooking(string userId, int roomId, DateTime checkIn, DateTime checkOut);
        List<Booking> GetMyBookings(string userId);
        Booking? GetById(int id);
        void CancelBooking(int bookingId, string userId);
        
        // Updated to accept transaction ID for idempotency
        void ConfirmPayment(int bookingId, string transactionId);
        void RecordPayment(int bookingId, decimal amount, string method, string transactionId);
        
        List<Booking> GetFilteredBookings(DateTime? date, BookingStatus? status, string phoneNumber);
        void UpdateStatus(int bookingId, BookingStatus newStatus);

        public List<Booking> SearchBookingsByPhoneNumber(string phoneNumber);
    }
}
