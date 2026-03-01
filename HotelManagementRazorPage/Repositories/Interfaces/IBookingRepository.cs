using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IBookingRepository
    {
        void Add(Booking booking);
        Booking? GetById(int id);
        List<Booking> GetByCustomer(string userId);
        IQueryable<Booking> GetQuery();
        void UpdateStatus(int bookingId, BookingStatus status);
        void Save();

        public List<Booking> GetByCustomerPhoneNumber(string phoneNumber);

        /// Returns true if the given room already has an active (non-cancelled, non-completed) booking that overlaps [checkIn, checkOut)
        bool HasOverlapBooking(int roomId, DateTime checkIn, DateTime checkOut, int? excludeBookingId = null);
    }
}
