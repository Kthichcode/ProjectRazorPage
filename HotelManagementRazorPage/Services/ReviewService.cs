using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IBookingRepository _bookingRepo;

        public ReviewService(IReviewRepository reviewRepo, IBookingRepository bookingRepo)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
        }

        public void SubmitReview(int bookingId, int roomId, int rating, string? comment)
        {
            var booking = _bookingRepo.GetById(bookingId)
                ?? throw new Exception("Không tìm thấy đơn đặt phòng.");

            if (booking.Status != BookingStatus.Completed)
                throw new Exception("Chỉ có thể bình luận sau khi đã trả phòng.");

            if (_reviewRepo.GetByBookingId(bookingId) != null)
                throw new Exception("Bạn đã bình luận cho đơn đặt phòng này rồi.");

            var review = new Review
            {
                BookingId = bookingId,
                RoomId = roomId,
                Rating = rating,
                Comment = comment,
                IsApproved = null, // Pending manager approval
                CreatedAt = DateTime.UtcNow
            };
            _reviewRepo.Add(review);
            _reviewRepo.Save();
        }

        public void SetApproval(int reviewId, bool approved)
        {
            var review = _reviewRepo.GetById(reviewId)
                ?? throw new Exception("Không tìm thấy bình luận.");
            review.IsApproved = approved;
            review.ReviewedAt = DateTime.UtcNow;
            _reviewRepo.Update(review);
            _reviewRepo.Save();
        }

        public List<Review> GetApprovedByRoom(int roomId)
            => _reviewRepo.GetApprovedByRoomId(roomId);

        public List<Review> GetPending()
            => _reviewRepo.GetPending();

        public List<Review> GetAll()
            => _reviewRepo.GetAll();

        public Review? GetByBookingId(int bookingId)
            => _reviewRepo.GetByBookingId(bookingId);

        public bool CanUserReview(string userId, int roomId, out int eligibleBookingId)
        {
            eligibleBookingId = 0;

            // Find completed bookings for this room by this user
            var completedBookings = _bookingRepo.GetByCustomer(userId)
                .Where(b => b.Status == BookingStatus.Completed
                         && b.BookingRooms.Any(br => br.RoomId == roomId))
                .ToList();

            foreach (var booking in completedBookings)
            {
                // Check if this booking already has a review
                if (_reviewRepo.GetByBookingId(booking.Id) == null)
                {
                    eligibleBookingId = booking.Id;
                    return true;
                }
            }
            return false;
        }
    }
}
