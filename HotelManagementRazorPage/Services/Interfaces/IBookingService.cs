using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System;
using System.Collections.Generic;

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

        
        List<Booking> GetFilteredBookings(DateTime? date, BookingStatus? status, string phoneNumber, int? roomId = null);

        void UpdateStatus(int bookingId, BookingStatus newStatus);
        List<Booking> SearchBookingsByPhoneNumber(string phoneNumber);

        // Cancellation with refund flow
        void RequestCancellation(int bookingId, string userId);
        void ApproveCancel(int bookingId);
    }
}
