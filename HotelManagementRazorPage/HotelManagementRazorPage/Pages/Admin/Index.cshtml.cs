using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.DTOs;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public IndexModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardDto Data { get; set; } = new();

        public void OnGet()
        {
            Data = _dashboardService.GetDashboardData();
        }
    }
}
