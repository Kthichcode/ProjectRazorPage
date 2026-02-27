using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Admin.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IRoomTypeService _roomTypeService;

        public DeleteModel(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        public RoomType? RoomType { get; set; }

        [BindProperty]
        public int RoomTypeId { get; set; }

        public IActionResult OnGet(int id)
        {
            RoomType = _roomTypeService.GetById(id);
            if (RoomType == null) return NotFound();
            RoomTypeId = id;
            return Page();
        }

        public IActionResult OnPost()
        {
            var rt = _roomTypeService.GetById(RoomTypeId);
            if (rt != null)
            {
                try
                {
                    _roomTypeService.Delete(RoomTypeId);
                    TempData["Success"] = $"Loại phòng '{rt.Name}' đã được xóa.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Không thể xóa loại phòng: " + ex.Message;
                }
            }
            return RedirectToPage("/Admin/RoomTypes/Index");
        }
    }
}
