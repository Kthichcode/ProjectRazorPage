using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Entities
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
    }
}
