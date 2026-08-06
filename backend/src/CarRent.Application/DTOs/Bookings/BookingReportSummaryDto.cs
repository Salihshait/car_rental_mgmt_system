namespace CarRent.Application.DTOs.Bookings;

public class BookingReportSummaryDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public Dictionary<string, int> BookingTypeCounts { get; set; } = new();
    public List<TopVehicleDto> TopVehicles { get; set; } = new();
}

public class TopVehicleDto
{
    public Guid VehicleId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}
