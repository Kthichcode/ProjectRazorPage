
using BusinessObjects.Entities;
using Services.DTOs;

namespace Services.Interfaces
{
    public interface IDashboardService
    {
        DashboardDto GetDashboardData();
        ManagerDashboardDto GetManagerDashboardData();
    }
}
