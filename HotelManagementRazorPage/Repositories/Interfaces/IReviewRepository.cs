using BusinessObjects.Entities;

namespace Repositories.Interfaces
{
    public interface IReviewRepository
    {
        void Add(Review review);
        void Update(Review review);
        Review? GetById(int id);
        Review? GetByBookingId(int bookingId);
        List<Review> GetByRoomId(int roomId);
        List<Review> GetApprovedByRoomId(int roomId);
        List<Review> GetPending();
        List<Review> GetAll();
        void Save();
    }
}
