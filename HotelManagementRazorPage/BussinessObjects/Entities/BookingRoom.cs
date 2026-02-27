using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class BookingRoom
    {
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
