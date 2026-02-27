using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        // Identity user id là string
        [Required] public string CustomerId { get; set; } = "";
        public ApplicationUser? Customer { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; } // ngày rời đi

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public Review? Review { get; set; }
    }
}
