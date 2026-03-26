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

    public class StatisticsDto
    {
        // Chart labels: "T1/2025", "T2/2025", ...
        public string[] MonthLabels { get; set; } = Array.Empty<string>();

        // Monthly series (12 entries, oldest-first)
        public int[]     NewUsersPerMonth    { get; set; } = Array.Empty<int>();
        public decimal[] RevenuePerMonth     { get; set; } = Array.Empty<decimal>();
        public int[]     BookingsPerMonth    { get; set; } = Array.Empty<int>();

        // Summary totals
        public int     TotalUsers    { get; set; }
        public decimal TotalRevenue  { get; set; }
        public int     TotalBookings { get; set; }
        public int     TotalRooms    { get; set; }

        // Selected year for the year filter
        public int SelectedYear { get; set; }
    }
}
