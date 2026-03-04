using BusinessObjects.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeService _roomTypeService;
        private readonly IReviewService _reviewService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IRoomService roomService, IRoomTypeService roomTypeService,
                          IReviewService reviewService, ILogger<IndexModel> logger)
        {
            _roomService = roomService;
            _roomTypeService = roomTypeService;
            _reviewService = reviewService;
            _logger = logger;
        }

        public List<Room> Rooms { get; set; } = new();
        public List<RoomType> RoomTypes { get; set; } = new();

        /// roomId -> (averageRating, reviewCount)
        public Dictionary<int, (double Avg, int Count)> RoomRatings { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckIn { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CheckOut { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedRoomTypeId { get; set; }

        public bool IsSearching { get; set; }

        public void OnGet()
        {
            RoomTypes = _roomTypeService.GetAll();

            if (CheckIn.HasValue && CheckOut.HasValue && CheckIn < CheckOut)
            {
                IsSearching = true;
                Rooms = _roomService.GetAvailableRooms(CheckIn.Value, CheckOut.Value, SelectedRoomTypeId);
            }
            else
            {
                Rooms = _roomService.GetAll();
            }

            // Load all approved reviews grouped by RoomId for star display
            RoomRatings = _reviewService.GetAll()
                .Where(r => r.IsApproved == true)
                .GroupBy(r => r.RoomId)
                .ToDictionary(
                    g => g.Key,
                    g => (Avg: g.Average(r => (double)r.Rating), Count: g.Count())
                );
        }
    }
}
