using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Admin.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IRoomTypeService _roomTypeService;

        public IndexModel(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        public List<RoomType> RoomTypes { get; set; } = new();

        public void OnGet()
        {
            RoomTypes = _roomTypeService.GetAll();
        }
    }
}
