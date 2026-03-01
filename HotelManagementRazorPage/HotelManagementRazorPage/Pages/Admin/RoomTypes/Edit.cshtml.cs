using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Admin.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IRoomTypeService _roomTypeService;

        public EditModel(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập tên loại phòng.")]
            [MaxLength(100)]
            public string Name { get; set; } = "";

            public string? Description { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập giá phòng.")]
            [Range(0, 999999999, ErrorMessage = "Giá không hợp lệ.")]
            [Display(Name = "Giá / đêm")]
            public decimal PricePerNight { get; set; }
        }

        public IActionResult OnGet(int id)
        {
            var rt = _roomTypeService.GetById(id);
            if (rt == null) return NotFound();

            Input = new InputModel
            {
                Id = rt.Id,
                Name = rt.Name,
                Description = rt.Description,
                PricePerNight = rt.PricePerNight
            };
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var rt = _roomTypeService.GetById(Input.Id);
            if (rt == null) return NotFound();

            rt.Name = Input.Name;
            rt.Description = Input.Description;
            rt.PricePerNight = Input.PricePerNight;

            _roomTypeService.Update(rt);
            TempData["Success"] = $"Loại phòng '{rt.Name}' đã được cập nhật.";
            return RedirectToPage("/Admin/RoomTypes/Index");
        }
    }
}
