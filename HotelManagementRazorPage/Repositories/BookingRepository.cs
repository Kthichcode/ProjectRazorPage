using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Booking booking)
        {
            _context.Bookings.Add(booking);
        }

        public Booking? GetById(int id)
        {
            return _context.Bookings
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.Customer)
                .FirstOrDefault(b => b.Id == id);
        }

        public List<Booking> GetByCustomer(string userId)
        {
            return _context.Bookings
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                .Include(b => b.Review)
                .Where(b => b.CustomerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }

        public IQueryable<Booking> GetQuery()
        {
            return _context.Bookings
                .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.Customer);
        }

        public void UpdateStatus(int bookingId, BookingStatus status)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking != null)
            {
                booking.Status = status;
            }
        }

        public List<Booking> GetByCustomerPhoneNumber(string phoneNumber)
        {
            return _context.Bookings
                .Include(b => b.Customer)
                .Where(b => b.Customer.PhoneNumber == phoneNumber)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }


        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
