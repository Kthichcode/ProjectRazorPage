using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRoomService roomService, IRoomTypeService roomTypeService, ILogger<IndexModel> logger)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _logger = logger;
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
            }
        }
    }
}
