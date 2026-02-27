using BusinessObjects.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _repo;

        public RoomTypeService(IRoomTypeRepository repo)
        {
            _repo = repo;
        }

        public List<RoomType> GetAll()
        {
            return _repo.GetAll();
        }

        public RoomType? GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(RoomType roomType)
        {
            _repo.Add(roomType);
            _repo.Save();
        }

        public void Update(RoomType roomType)
        {
            _repo.Update(roomType);
            _repo.Save();
        }

        public void Delete(int id)
        {
            RoomType? existing = _repo.GetById(id);
            if (existing == null)
            {
                return;
            }

            _repo.Delete(existing);
            _repo.Save();
        }
    }
}
