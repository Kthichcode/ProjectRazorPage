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
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Room> GetAll()
        {
            return _context.Rooms
                .Include(r => r.RoomType)
                .ToList();
        }

        public Room? GetById(int id)
        {
            return _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefault(r => r.Id == id);
        }

        public void Add(Room room)
        {
            _context.Rooms.Add(room);
        }

        public void Update(Room room)
        {
            _context.Rooms.Update(room);
        }

        public void Delete(Room room)
        {
            _context.Rooms.Remove(room);
        }

        public void Save()
        {
            _context.SaveChanges();
        }



        public IQueryable<Room> Query()
        {
            return _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.BookingRooms)
                    .ThenInclude(br => br.Booking);
        }

        public List<Room> GetAllWithBookings()
        {
            return _context.Rooms
                 .Include(r => r.RoomType)
                 .Include(r => r.BookingRooms)
                     .ThenInclude(br => br.Booking)
                 .ToList();
        }


        public bool ExistsRoomNumber(string roomNumber)
        {
            string key = roomNumber.Trim();
            Room? room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == key);
            return room != null;
        }

        public bool ExistsRoomNumberExceptId(string roomNumber, int roomId)
        {
            string key = roomNumber.Trim();
            Room? room = _context.Rooms.FirstOrDefault(r => r.RoomNumber == key && r.Id != roomId);
            return room != null;
        }

        public Room? GetByIdWithImages(int id)
        {
            return _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomImages)
                .FirstOrDefault(r => r.Id == id);
        }
    }
}
