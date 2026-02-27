using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Admin.RoomTypes
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IRoomTypeService _roomTypeService;

        public CreateModel(IRoomTypeService roomTypeService)
        {
            _roomTypeService = roomTypeService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên loại phòng.")]
            [MaxLength(100)]
            public string Name { get; set; } = "";

            public string? Description { get; set; }

            [Required]
            [Range(0, 999999999, ErrorMessage = "Giá không hợp lệ.")]
            [Display(Name = "Giá / đêm")]
            public decimal PricePerNight { get; set; }
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            var rt = new RoomType
            {
                Name = Input.Name,
                Description = Input.Description,
                PricePerNight = Input.PricePerNight
            };

            _roomTypeService.Create(rt);
            TempData["Success"] = $"Loại phòng '{rt.Name}' đã được thêm thành công.";
            return RedirectToPage("/Admin/RoomTypes/Index");
        }
    }
}
