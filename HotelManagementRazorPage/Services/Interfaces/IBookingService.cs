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

        /// Wallet-only or hybrid Wallet+VNPay payment confirmation
        /// walletAmountPaid: amount already deducted from wallet before going to VNPay (0 if pure VNPay)
        void ConfirmPaymentWithWallet(int bookingId, decimal walletAmountPaid, string vnpayTransactionId);

        /// Confirm full wallet payment (no VNPay needed)
        void ConfirmFullWalletPayment(int bookingId);

        /// Persist changes made to a booking entity (e.g. WalletAmountPaid)
        void SaveBookingChanges(int bookingId, Booking booking);

        
        List<Booking> GetFilteredBookings(DateTime? date, BookingStatus? status, string phoneNumber, int? roomId = null);

        void UpdateStatus(int bookingId, BookingStatus newStatus);
        List<Booking> SearchBookingsByPhoneNumber(string phoneNumber);

        // Cancellation with refund flow
        void RequestCancellation(int bookingId, string userId);
        void ApproveCancel(int bookingId);
    }
}
