using BusinessObjects.Entities;

namespace Services.Interfaces
{
    public interface IReviewService
    {
        /// Submit a review for a booking (only if booking is Completed and no review exists yet)
        void SubmitReview(int bookingId, int roomId, int rating, string? comment);

        /// Manager: approve or reject a review
        void SetApproval(int reviewId, bool approved);

        /// Get all approved reviews for a room (public)
        List<Review> GetApprovedByRoom(int roomId);

        /// Get all pending reviews (manager)
        List<Review> GetPending();

        /// Get all reviews (manager)
        List<Review> GetAll();

        /// Get review by booking id
        Review? GetByBookingId(int bookingId);

        /// Check if user can review a specific room 
        bool CanUserReview(string userId, int roomId, out int eligibleBookingId);
    }
}
