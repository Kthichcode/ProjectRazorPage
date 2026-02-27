using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IRoomTypeService
    {
        List<RoomType> GetAll();
        RoomType? GetById(int id);
        void Create(RoomType roomType);
        void Update(RoomType roomType);
        void Delete(int id);
    }
}
