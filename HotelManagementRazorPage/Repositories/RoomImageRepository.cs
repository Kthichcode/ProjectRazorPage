using BusinessObjects.Entities;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RoomImageRepository : IRoomImageRepository
    {
        private readonly AppDbContext _db;

        public RoomImageRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<RoomImage> GetByRoomId(int roomId)
        {
            return _db.RoomImages.Where(x => x.RoomId == roomId).ToList();
        }

        public void AddRange(List<RoomImage> images)
        {
            _db.RoomImages.AddRange(images);
        }

        public void DeleteRange(List<RoomImage> images)
        {
            _db.RoomImages.RemoveRange(images);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
