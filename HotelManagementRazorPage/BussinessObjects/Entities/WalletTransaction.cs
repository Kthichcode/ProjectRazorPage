using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Entities
{
    public enum WalletTransactionType
    {
        Refund = 1,
        Deposit = 2,
        Payment = 3
    }

    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }

        public int WalletId { get; set; }

        [ForeignKey("WalletId")]
        public virtual Wallet? Wallet { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public WalletTransactionType Type { get; set; }

        public string Description { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
