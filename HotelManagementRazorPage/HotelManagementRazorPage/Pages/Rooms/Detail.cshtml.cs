using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.Security.Claims;

namespace HotelManagementRazorPage.Pages.Rooms
{
    public class DetailModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IReviewService _reviewService;

        public DetailModel(IRoomService roomService, IReviewService reviewService)
        {
            _roomService = roomService;
            _reviewService = reviewService;
        }

        public Room? Room { get; set; }
        public List<RoomImage> ExtraImages { get; set; } = new();
        public List<Review> ApprovedReviews { get; set; } = new();

        // Review eligibility
        public bool CanReview { get; set; }
        public int EligibleBookingId { get; set; }

        // Form submission
        [BindProperty] public int ReviewRating { get; set; } = 5;
        [BindProperty] public string? ReviewComment { get; set; }
        [BindProperty] public int ReviewBookingId { get; set; }
        [BindProperty] public int ReviewRoomId { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(int id)
        {
            Room = _roomService.GetByIdWithImages(id);
            if (Room == null) return NotFound();

            ExtraImages = Room.RoomImages.ToList();
            ApprovedReviews = _reviewService.GetApprovedByRoom(id);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                CanReview = _reviewService.CanUserReview(userId, id, out int bookingId);
                EligibleBookingId = bookingId;
            }

            if (TempData["ReviewSuccess"] != null) SuccessMessage = TempData["ReviewSuccess"]?.ToString();
            if (TempData["ReviewError"] != null) ErrorMessage = TempData["ReviewError"]?.ToString();

            return Page();
        }

        public IActionResult OnPostReview(int id)
        {
            try
            {
                if (ReviewRating < 1 || ReviewRating > 5)
                    throw new Exception("Đánh giá phải từ 1 đến 5 sao.");
                if (string.IsNullOrWhiteSpace(ReviewComment))
                    throw new Exception("Vui lòng nhập nội dung bình luận.");

                _reviewService.SubmitReview(ReviewBookingId, ReviewRoomId, ReviewRating, ReviewComment);
                TempData["ReviewSuccess"] = "Bình luận của bạn đã được gửi và đang chờ duyệt.";
            }
            catch (Exception ex)
            {
                TempData["ReviewError"] = ex.Message;
            }
            return RedirectToPage(new { id });
        }
    }
}
