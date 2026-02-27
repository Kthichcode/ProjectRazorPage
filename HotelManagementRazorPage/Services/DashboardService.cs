using BusinessObjects.Enums;
using Repositories.Interfaces;
using Services.DTOs;
using Services.Interfaces;
using System;
using System.Linq;

namespace Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;

        public DashboardService(IBookingRepository bookingRepo, IRoomRepository roomRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
        }

        public DashboardDto GetDashboardData()
        {
            var dto = new DashboardDto();
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // 1. Room Stats
            var rooms = _roomRepo.GetAll();
            dto.TotalRooms = rooms.Count;
            dto.AvailableRooms = rooms.Count(r => r.Status == RoomStatus.Available);
            dto.MaintenanceRooms = rooms.Count(r => r.Status == RoomStatus.Maintenance);

            // 2. Booking Stats
            var bookingsQuery = _bookingRepo.GetQuery();

            dto.BookingsToday = bookingsQuery.Count(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1));
            dto.BookingsThisMonth = bookingsQuery.Count(b => b.CreatedAt >= startOfMonth);

            // 3. Revenue Stats (Only Confirmed, CheckedIn, Completed)
            var paidStatuses = new[] { BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed };
            
            var paidBookings = bookingsQuery.Where(b => paidStatuses.Contains(b.Status));

            dto.RevenueToday = paidBookings
                .Where(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1))
                .Sum(b => b.TotalAmount);

            dto.RevenueThisMonth = paidBookings
                .Where(b => b.CreatedAt >= startOfMonth)
                .Sum(b => b.TotalAmount);

            // 4. Top Room Types
            var topTypes = bookingsQuery
                .SelectMany(b => b.BookingRooms)
                .GroupBy(br => br.Room.RoomType.Name)
                .Select(g => new TopRoomTypeDto
                {
                    RoomTypeName = g.Key,
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToList();

            dto.TopRoomTypes = topTypes;

            return dto;
        }
    }
}
