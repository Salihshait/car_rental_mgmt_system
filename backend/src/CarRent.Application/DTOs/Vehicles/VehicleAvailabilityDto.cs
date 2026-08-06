namespace CarRent.Application.DTOs.Vehicles;

public class BookedRangeDto
{
    public Guid BookingId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class VehicleAvailabilityDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public List<BookedRangeDto> BookedRanges { get; set; } = new();
}
