using BusinessObjects.Entities;

namespace Services.DTOs
{
    public class ManagerDashboardDto
    {
        // Stat cards
        public int BookingsToday { get; set; }
        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
        public int CurrentlyCheckedIn { get; set; }
        public int PendingCount { get; set; }

        // Refund / cancellation pending
        public int CancellationPendingCount { get; set; }
        public List<Booking> CancellationPendingBookings { get; set; } = new();

        // Action lists
        public List<Booking> PendingBookings { get; set; } = new();
        public List<Booking> CheckIngToday { get; set; } = new();
        public List<Booking> CheckingOutToday { get; set; } = new();
    }
}
