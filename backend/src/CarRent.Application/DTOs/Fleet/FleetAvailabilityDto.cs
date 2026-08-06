namespace CarRent.Application.DTOs.Fleet;

public class FleetAvailabilityDto
{
    public Guid VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string? BranchName { get; set; }
    public string VehicleStatus { get; set; } = string.Empty;
    public string FleetAvailabilityStatus { get; set; } = string.Empty;
}
