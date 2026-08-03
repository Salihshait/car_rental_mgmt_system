namespace CarRent.Application.DTOs.Dashboard;

public class DashboardMetricsDto
{
    public decimal Revenue { get; set; }
    public int ActiveRentals { get; set; }
    public int AvailableCars { get; set; }
    public int BookingsThisMonth { get; set; }
}
