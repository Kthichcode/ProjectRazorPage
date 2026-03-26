using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.DTOs;
using Services.Interfaces;
using System;

namespace HotelManagementRazorPage.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class StatisticsModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public StatisticsModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public StatisticsDto Stats { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Now.Year;

        public void OnGet()
        {
            // Clamp year to a sane range
            if (Year < 2020 || Year > DateTime.Now.Year + 1)
                Year = DateTime.Now.Year;

            Stats = _dashboardService.GetStatisticsData(Year);
        }
    }
}
