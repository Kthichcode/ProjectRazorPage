using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IRoomRepository
    {
        IQueryable<Room> Query();
        List<Room> GetAllWithBookings();
        List<Room> GetAll();
        Room? GetById(int id);
        void Add(Room room);
        void Update(Room room);
        void Delete(Room room);
        void Save();
        bool ExistsRoomNumber(string roomNumber);
        bool ExistsRoomNumberExceptId(string roomNumber, int roomId);
        Room? GetByIdWithImages(int id);


    }
}
