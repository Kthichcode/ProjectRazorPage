using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Manager.Rooms
{
    [Authorize(Roles = "Manager,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IWebHostEnvironment _env;
        private readonly ISignalRService _signalRService;

        public CreateModel(IRoomService roomService, IRoomTypeService roomTypeService, IWebHostEnvironment env, ISignalRService signalRService)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _env = env;
            _signalRService = signalRService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public List<IFormFile>? ImageFiles { get; set; }

        public List<SelectListItem> RoomTypeList { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập số phòng.")]
            [Display(Name = "Số phòng")]
            public string RoomNumber { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng chọn loại phòng.")]
            [Display(Name = "Loại phòng")]
            public int RoomTypeId { get; set; }

            [Required]
            [Range(1, 20, ErrorMessage = "Sức chứa từ 1 đến 20 khách.")]
            [Display(Name = "Sức chứa tối đa")]
            public int MaxOccupancy { get; set; } = 2;

            public RoomStatus Status { get; set; } = RoomStatus.Available;

            public string? Description { get; set; }
        }

        public void OnGet()
        {
            LoadRoomTypes();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_roomService.IsRoomNumberExists(Input.RoomNumber))
                ModelState.AddModelError("Input.RoomNumber", "Số phòng này đã tồn tại.");

            if (!ModelState.IsValid)
            {
                LoadRoomTypes();
                return Page();
            }

            var savedUrls = new List<string>();
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                foreach (var file in ImageFiles.Where(f => f.Length > 0))
                {
                    var url = await SaveImageAsync(file);
                    savedUrls.Add(url);
                }
            }

            var room = new Room
            {
                RoomNumber = Input.RoomNumber,
                RoomTypeId = Input.RoomTypeId,
                MaxOccupancy = Input.MaxOccupancy,
                Status = Input.Status,
                ImageUrl = savedUrls.Count > 0 ? savedUrls[0] : "",
                Description = Input.Description
            };

            _roomService.Create(room);

            if (savedUrls.Count > 1)
                _roomService.AddRoomImages(room.Id, savedUrls.Skip(1).ToList());

            TempData["Success"] = $"Phòng {room.RoomNumber} đã được thêm thành công" +
                                  (savedUrls.Count > 0 ? $" với {savedUrls.Count} ảnh." : ".");
            try { await _signalRService.SendRoomUpdate("created", room.RoomNumber); } catch { }
            return RedirectToPage("/Manager/Rooms/Index");
        }

        private void LoadRoomTypes()
        {
            RoomTypeList = _roomTypeService.GetAll()
                .Select(rt => new SelectListItem(rt.Name, rt.Id.ToString()))
                .ToList();
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) ext = ".jpg";

            var uploadDir = Path.Combine(_env.WebRootPath, "images", "rooms");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/rooms/{fileName}";
        }
    }
}
