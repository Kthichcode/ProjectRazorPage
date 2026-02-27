using System.Collections.Generic;

namespace Services.DTOs
{
    public class DashboardDto
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int BookingsToday { get; set; }
        public int BookingsThisMonth { get; set; }

        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }

        public List<TopRoomTypeDto> TopRoomTypes { get; set; } = new List<TopRoomTypeDto>();
    }

    public class TopRoomTypeDto
    {
        public string RoomTypeName { get; set; } = "";
        public int BookingCount { get; set; }
    }
}
