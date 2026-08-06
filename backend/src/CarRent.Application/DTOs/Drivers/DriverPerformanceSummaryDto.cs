namespace CarRent.Application.DTOs.Drivers;

public class DriverPerformanceSummaryDto
{
    public Guid DriverId { get; set; }
    public int TripsThisMonth { get; set; }
    public decimal DistanceThisMonthKm { get; set; }
    public double AttendanceRateThisMonth { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public string? CurrentVehicleRegistrationNumber { get; set; }
}
