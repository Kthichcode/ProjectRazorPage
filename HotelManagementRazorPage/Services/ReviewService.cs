using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool CanReview(int bookingId, string userId)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) return false;
            
            // Chỉ cho phép review khi booking đã Completed và thuộc về user này
            if (booking.Status != BookingStatus.Completed) return false;
            if (booking.CustomerId != userId) return false;
            
            // Kiểm tra xem đã có review chưa
            var existingReview = _reviewRepo.GetByBookingId(bookingId);
            return existingReview == null;
        }

        public int CreateReview(int bookingId, int rating, string? comment)
        {
            var booking = _bookingRepo.GetById(bookingId);
            if (booking == null) throw new Exception("Booking not found.");
            
            // Kiểm tra booking đã Completed chưa
            if (booking.Status != BookingStatus.Completed)
                throw new Exception("Chỉ có thể đánh giá các đặt phòng đã hoàn thành.");
            
            // Kiểm tra đã có review chưa
            var existingReview = _reviewRepo.GetByBookingId(bookingId);
            if (existingReview != null)
                throw new Exception("Bạn đã đánh giá đặt phòng này rồi.");
            
            // Validate rating
            if (rating < 1 || rating > 5)
                throw new Exception("Đánh giá phải từ 1 đến 5 sao.");
            
            var review = new Review
            {
                BookingId = bookingId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
            
            _reviewRepo.Add(review);
            _reviewRepo.Save();
            
            return review.Id;
        }

        public Review? GetById(int id)
        {
            return _reviewRepo.GetById(id);
        }

        public Review? GetByBookingId(int bookingId)
        {
            return _reviewRepo.GetByBookingId(bookingId);
        }

        public List<Review> GetByRoomId(int roomId)
        {
            return _reviewRepo.GetByRoomId(roomId);
        }

        public List<Review> GetAll()
        {
            return _reviewRepo.GetAll();
        }
    }
}

