using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Admin.Rooms
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IWebHostEnvironment _env;

        public EditModel(IRoomService roomService, IRoomTypeService roomTypeService, IWebHostEnvironment env)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public List<IFormFile>? ImageFiles { get; set; }

        public List<SelectListItem> RoomTypeList { get; set; } = new();

        // Extra images (not the thumbnail) for display only
        public List<RoomImage> ExistingImages { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số phòng.")]
            public string RoomNumber { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng chọn loại phòng.")]
            public int RoomTypeId { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập sức chứa.")]
            [Range(1, 20, ErrorMessage = "Sức chứa từ 1 đến 20 khách.")]
            public int MaxOccupancy { get; set; }

            public RoomStatus Status { get; set; }

            // Keep thumbnail URL (read-only — not editable via URL, only via file upload)
            public string ImageUrl { get; set; } = "";

            public string? Description { get; set; }
        }

        public IActionResult OnGet(int id)
        {
            var room = _roomService.GetByIdWithImages(id);
            if (room == null) return NotFound();

            Input = new InputModel
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomTypeId = room.RoomTypeId,
                MaxOccupancy = room.MaxOccupancy,
                Status = room.Status,
                ImageUrl = room.ImageUrl,
                Description = room.Description
            };

            ExistingImages = room.RoomImages.ToList();
            LoadRoomTypes();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_roomService.IsRoomNumberExistsExceptId(Input.RoomNumber, Input.Id))
                ModelState.AddModelError("Input.RoomNumber", "Số phòng này đã tồn tại.");

            if (!ModelState.IsValid)
            {
                // Reload extra images for display
                var r = _roomService.GetByIdWithImages(Input.Id);
                ExistingImages = r?.RoomImages.ToList() ?? new();
                LoadRoomTypes();
                return Page();
            }

            var room = _roomService.GetByIdWithImages(Input.Id);
            if (room == null) return NotFound();

            // If new images were uploaded → replace all existing
            if (ImageFiles != null && ImageFiles.Any(f => f.Length > 0))
            {
                var validFiles = ImageFiles.Where(f => f.Length > 0).ToList();

                // Delete old local files (thumbnail + extra images)
                DeleteLocalFile(room.ImageUrl);
                foreach (var img in room.RoomImages)
                    DeleteLocalFile(img.ImageUrl);

                // Save new images
                var newUrls = new List<string>();
                foreach (var file in validFiles)
                    newUrls.Add(await SaveImageAsync(file));

                room.ImageUrl = newUrls[0];
                _roomService.ReplaceRoomImages(room.Id,
                    newUrls.Count > 1 ? newUrls.Skip(1).ToList() : new List<string>());

                TempData["Success"] = $"Phòng {room.RoomNumber} đã được cập nhật với {newUrls.Count} ảnh mới.";
            }
            else
            {
                TempData["Success"] = $"Phòng {Input.RoomNumber} đã được cập nhật.";
            }

            room.RoomNumber = Input.RoomNumber;
            room.RoomTypeId = Input.RoomTypeId;
            room.MaxOccupancy = Input.MaxOccupancy;
            room.Status = Input.Status;
            room.Description = Input.Description;

            _roomService.Update(room);
            return RedirectToPage("/Admin/Rooms/Index");
        }

        private void LoadRoomTypes()
        {
            RoomTypeList = _roomTypeService.GetAll()
                .Select(rt => new SelectListItem(rt.Name, rt.Id.ToString()))
                .ToList();
        }

        private void DeleteLocalFile(string? url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/images/rooms/")) return;
            var path = Path.Combine(_env.WebRootPath,
                url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) ext = ".jpg";

            var dir = Path.Combine(_env.WebRootPath, "images", "rooms");
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/rooms/{fileName}";
        }
    }
}
