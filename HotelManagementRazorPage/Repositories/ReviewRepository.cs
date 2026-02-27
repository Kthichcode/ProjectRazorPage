using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Review review)
        {
            _context.Reviews.Add(review);
        }

        public Review? GetById(int id)
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.BookingRooms)
                        .ThenInclude(br => br.Room)
                .FirstOrDefault(r => r.Id == id);
        }

        public Review? GetByBookingId(int bookingId)
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.BookingRooms)
                        .ThenInclude(br => br.Room)
                .FirstOrDefault(r => r.BookingId == bookingId);
        }

        public List<Review> GetByRoomId(int roomId)
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.BookingRooms)
                        .ThenInclude(br => br.Room)
                .Where(r => r.Booking.BookingRooms.Any(br => br.RoomId == roomId))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetAll()
        {
            return _context.Reviews
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.BookingRooms)
                        .ThenInclude(br => br.Room)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

