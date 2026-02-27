using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;

        public IndexModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public List<Room> Rooms { get; set; } = new();

        public void OnGet()
        {
            Rooms = _roomService.GetAll();
        }
    }
}
