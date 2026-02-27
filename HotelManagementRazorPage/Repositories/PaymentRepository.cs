using BusinessObjects.Entities;
using Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Payment payment)
        {
            _context.Payments.Add(payment);
        }

        public List<Payment> GetByBookingId(int bookingId)
        {
            return _context.Payments.Where(p => p.BookingId == bookingId).ToList();
        }

        public Payment? GetByTransactionId(string transactionId)
        {
            return _context.Payments.FirstOrDefault(p => p.ProviderTransactionId == transactionId && p.ProviderTransactionId != null);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
