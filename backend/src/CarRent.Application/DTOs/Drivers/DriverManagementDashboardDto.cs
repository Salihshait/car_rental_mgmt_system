namespace CarRent.Application.DTOs.Drivers;

public class DriverManagementDashboardDto
{
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public int OnLeaveToday { get; set; }
    public int LicensesExpiringSoonCount { get; set; }
    public int LicensesExpiredCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int TotalTripsThisMonth { get; set; }
}
