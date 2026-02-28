using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager,Admin")]
    public class DeleteModel : PageModel
    {
        private readonly IRoomService _roomService;

        public DeleteModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public Room? Room { get; set; }

        [BindProperty]
        public int RoomId { get; set; }

        public IActionResult OnGet(int id)
        {
            Room = _roomService.GetById(id);
            if (Room == null) return NotFound();
            RoomId = id;
            return Page();
        }

        public IActionResult OnPost()
        {
            var room = _roomService.GetById(RoomId);
            if (room != null)
            {
                try
                {
                    _roomService.Delete(RoomId);
                    TempData["Success"] = $"Phòng {room.RoomNumber} đã được xóa thành công.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Không thể xóa phòng này: " + ex.Message;
                }
            }
            return RedirectToPage("/Manager/Rooms/Index");
        }
    }
}
