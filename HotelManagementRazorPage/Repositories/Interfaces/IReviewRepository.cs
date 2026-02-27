using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IReviewRepository
    {
        void Add(Review review);
        Review? GetById(int id);
        Review? GetByBookingId(int bookingId);
        List<Review> GetByRoomId(int roomId);
        List<Review> GetAll();
        void Save();
    }
}

