using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IReviewService
    {
        int CreateReview(int bookingId, int rating, string? comment);
        Review? GetById(int id);
        Review? GetByBookingId(int bookingId);
        List<Review> GetByRoomId(int roomId);
        List<Review> GetAll();
        bool CanReview(int bookingId, string userId);
    }
}

