using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Add(Review review) => _context.Reviews.Add(review);

        public void Update(Review review) => _context.Reviews.Update(review);

        public Review? GetById(int id)
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.Id == id);
        }

        public Review? GetByBookingId(int bookingId)
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Include(r => r.Room)
                .FirstOrDefault(r => r.BookingId == bookingId);
        }

        public List<Review> GetByRoomId(int roomId)
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Where(r => r.RoomId == roomId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetApprovedByRoomId(int roomId)
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Where(r => r.RoomId == roomId && r.IsApproved == true)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetPending()
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Include(r => r.Room)
                .Where(r => r.IsApproved == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetAll()
        {
            return _context.Reviews
                .Include(r => r.Booking).ThenInclude(b => b != null ? b.Customer : null!)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public void Save() => _context.SaveChanges();
    }
}
