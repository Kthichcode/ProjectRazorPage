using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }

        [Range(0, 999999999)]
        public decimal PricePerNight { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
