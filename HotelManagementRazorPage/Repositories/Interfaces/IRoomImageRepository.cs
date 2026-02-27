using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IRoomImageRepository
    {
        List<RoomImage> GetByRoomId(int roomId);
        void AddRange(List<RoomImage> images);
        void DeleteRange(List<RoomImage> images);
        void Save();
    }
}
