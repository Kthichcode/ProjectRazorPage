using BusinessObjects.Entities;
using System.Collections.Generic;

namespace Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        void Add(Payment payment);
        List<Payment> GetByBookingId(int bookingId);
        Payment? GetByTransactionId(string transactionId);
        void Save();
    }
}
