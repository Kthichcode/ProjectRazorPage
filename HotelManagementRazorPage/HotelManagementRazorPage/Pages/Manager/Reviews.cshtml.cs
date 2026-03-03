using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Manager
{
    [Authorize(Roles = "Manager,Admin")]
    public class ReviewsModel : PageModel
    {
        private readonly IReviewService _reviewService;

        public ReviewsModel(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        public List<Review> PendingReviews { get; set; } = new();
        public List<Review> AllReviews { get; set; } = new();
        public string Tab { get; set; } = "pending";

        public void OnGet(string tab = "pending")
        {
            Tab = tab;
            PendingReviews = _reviewService.GetPending();
            AllReviews = _reviewService.GetAll();
        }

        public IActionResult OnPostApprove(int reviewId, string tab = "pending")
        {
            try { _reviewService.SetApproval(reviewId, true); TempData["Success"] = "Đã duyệt bình luận."; }
            catch { TempData["Error"] = "Có lỗi xảy ra."; }
            return RedirectToPage(new { tab });
        }

        public IActionResult OnPostReject(int reviewId, string tab = "pending")
        {
            try { _reviewService.SetApproval(reviewId, false); TempData["Success"] = "Đã từ chối bình luận."; }
            catch { TempData["Error"] = "Có lỗi xảy ra."; }
            return RedirectToPage(new { tab });
        }
    }
}
