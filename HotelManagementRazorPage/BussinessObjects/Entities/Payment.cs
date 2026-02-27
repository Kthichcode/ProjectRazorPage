using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = "Cash"; // Cash / VNPay / MoMo

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // dùng cho VNPay/MoMo sau
        public string? TxnRef { get; set; }                 // mã gửi cổng
        public string? ProviderTransactionId { get; set; }   // mã trả về
        public string? RawCallbackData { get; set; }         // lưu callback để debug

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
