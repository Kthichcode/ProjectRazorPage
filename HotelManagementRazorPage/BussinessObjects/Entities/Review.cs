using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        /// RoomId for easy querying without joining BookingRooms
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// false = chờ duyệt, true = đã duyệt, null = bị từ chối
        public bool? IsApproved { get; set; } = null; // null = Pending

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
