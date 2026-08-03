using CarRent.Application.DTOs.Dashboard;

namespace CarRent.Application.Services;

public class DashboardService
{
    public DashboardMetricsDto GetMetrics()
    {
        return new DashboardMetricsDto
        {
            Revenue = 1250000m,
            ActiveRentals = 128,
            AvailableCars = 94,
            BookingsThisMonth = 248
        };
    }
}
