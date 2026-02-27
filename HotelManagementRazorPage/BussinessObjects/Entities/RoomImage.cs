using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class RoomImage
    {
        [Key]
        public int Id { get; set; }

        public int RoomId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = "";

        [ForeignKey(nameof(RoomId))]
        public Room? Room { get; set; }
    }
}
