using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;

        public IndexModel(IRoomService roomService, IRoomTypeService roomTypeService)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
        }

        public List<Room> Rooms { get; set; } = new();
        public List<RoomType> RoomTypes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckIn { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckOut { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedRoomTypeId { get; set; }

        public bool IsSearching { get; set; }

        public void OnGet()
        {
            RoomTypes = _roomTypeService.GetAll();

            if (CheckIn.HasValue && CheckOut.HasValue && CheckIn < CheckOut)
            {
                IsSearching = true;
                Rooms = _roomService.GetAvailableRooms(CheckIn.Value, CheckOut.Value, SelectedRoomTypeId);
            }
            else
            {
                Rooms = _roomService.GetAll();
                // If only roomTypeId is set, filter by that
                if (SelectedRoomTypeId.HasValue)
                {
                    Rooms = Rooms.Where(r => r.RoomTypeId == SelectedRoomTypeId.Value).ToList();
                    IsSearching = true;
                }
            }
        }
    }
}
