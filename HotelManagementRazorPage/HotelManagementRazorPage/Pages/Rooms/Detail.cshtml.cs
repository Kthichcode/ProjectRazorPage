using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Rooms
{
    public class DetailModel : PageModel
    {
        private readonly IRoomService _roomService;

        public DetailModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public Room? Room { get; set; }
        public List<RoomImage> ExtraImages { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            Room = _roomService.GetByIdWithImages(id);
            if (Room == null) return NotFound();

            ExtraImages = Room.RoomImages.ToList();
            return Page();
        }
    }
}
