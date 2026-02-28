using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.DTOs;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Manager
{
    [Authorize(Roles = "Manager,Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public ManagerDashboardDto Data { get; set; } = new();

        public void OnGet()
        {
            Data = _dashboardService.GetManagerDashboardData();
        }
    }
}
