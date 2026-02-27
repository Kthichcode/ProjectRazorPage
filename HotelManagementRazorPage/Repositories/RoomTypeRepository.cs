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
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly AppDbContext _context;

        public RoomTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<RoomType> GetAll()
        {
            return _context.RoomTypes.AsNoTracking().ToList();
        }

        public RoomType? GetById(int id)
        {
            return _context.RoomTypes.FirstOrDefault(x => x.Id == id);
        }

        public void Add(RoomType roomType)
        {
            _context.RoomTypes.Add(roomType);
        }

        public void Update(RoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
        }

        public void Delete(RoomType roomType)
        {
            _context.RoomTypes.Remove(roomType);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}

