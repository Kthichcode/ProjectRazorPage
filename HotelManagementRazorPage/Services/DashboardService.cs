using BusinessObjects;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories;
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
        private readonly AppDbContext _db;

        public DashboardService(IBookingRepository bookingRepo, IRoomRepository roomRepo, AppDbContext db)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _db = db;
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

        public StatisticsDto GetStatisticsData(int year)
        {
            var paidStatuses = new[] { BookingStatus.Confirmed, BookingStatus.CheckedIn, BookingStatus.Completed };
            var bookingsQuery = _bookingRepo.GetQuery();

            // Build 12 months for the chosen calendar year (Jan–Dec)
            var months = Enumerable.Range(1, 12)
                .Select(m => new DateTime(year, m, 1))
                .ToArray();

            var labels = months.Select(m => $"T{m.Month}/{m.Year}").ToArray();

            // Revenue per month (paid bookings only)
            var revenuePerMonth = months.Select(m =>
                bookingsQuery
                    .Where(b => paidStatuses.Contains(b.Status)
                             && b.CreatedAt.Year == m.Year
                             && b.CreatedAt.Month == m.Month)
                    .Sum(b => (decimal?)b.TotalAmount) ?? 0m
            ).ToArray();

            // Bookings per month (all statuses)
            var bookingsPerMonth = months.Select(m =>
                bookingsQuery.Count(b => b.CreatedAt.Year == m.Year && b.CreatedAt.Month == m.Month)
            ).ToArray();

            // New users per month: users making their first booking that month (proxy)
            var firstBookingByUser = bookingsQuery
                .GroupBy(b => b.CustomerId)
                .Select(g => g.Min(b => b.CreatedAt))
                .ToList();

            var newUsersPerMonth = months.Select(m =>
                firstBookingByUser.Count(d => d.Year == m.Year && d.Month == m.Month)
            ).ToArray();

            // Totals (all-time, independent of year filter)
            int totalUsers = _db.Users.Count();
            decimal totalRevenue = bookingsQuery
                .Where(b => paidStatuses.Contains(b.Status))
                .Sum(b => (decimal?)b.TotalAmount) ?? 0m;
            int totalBookings = bookingsQuery.Count();
            int totalRooms = _roomRepo.GetAll().Count;

            return new StatisticsDto
            {
                MonthLabels      = labels,
                NewUsersPerMonth = newUsersPerMonth,
                RevenuePerMonth  = revenuePerMonth,
                BookingsPerMonth = bookingsPerMonth,
                TotalUsers       = totalUsers,
                TotalRevenue     = totalRevenue,
                TotalBookings    = totalBookings,
                TotalRooms       = totalRooms,
                SelectedYear     = year,
            };
        }

        public ManagerDashboardDto GetManagerDashboardData()
        {
            var dto = new ManagerDashboardDto();
            var today = DateTime.Today;

            var bookingsQuery = _bookingRepo.GetQuery();

            // Booking created today
            dto.BookingsToday = bookingsQuery
                .Count(b => b.CreatedAt >= today && b.CreatedAt < today.AddDays(1));

            // Check-in today: Confirmed + CheckInDate = today
            var checkInsToday = bookingsQuery
                .Where(b => b.CheckInDate.Date == today
                         && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.CheckedIn))
                .ToList();
            dto.CheckInsToday = checkInsToday.Count(b => b.Status == BookingStatus.Confirmed
                                                      || b.Status == BookingStatus.CheckedIn);

            // Check-out today: CheckedIn + CheckOutDate = today
            var checkOutsToday = bookingsQuery
                .Where(b => b.CheckOutDate.Date == today && b.Status == BookingStatus.CheckedIn)
                .ToList();
            dto.CheckOutsToday = checkOutsToday.Count;

            // Currently checked in
            dto.CurrentlyCheckedIn = bookingsQuery.Count(b => b.Status == BookingStatus.CheckedIn);

            // Pending bookings needing action
            var pending = bookingsQuery
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderBy(b => b.CheckInDate)
                .ToList();
            dto.PendingCount = pending.Count;
            dto.PendingBookings = pending;

            // Check-ins scheduled for today
            dto.CheckIngToday = bookingsQuery
                .Where(b => b.CheckInDate.Date == today && b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.CheckInDate)
                .ToList();

            // Check-outs for today
            dto.CheckingOutToday = checkOutsToday;

            // Cancellation / refund pending
            var cancellationPending = bookingsQuery
                .Where(b => b.Status == BookingStatus.CancellationPending)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            dto.CancellationPendingCount = cancellationPending.Count;
            dto.CancellationPendingBookings = cancellationPending;

            return dto;
        }
    }
}
