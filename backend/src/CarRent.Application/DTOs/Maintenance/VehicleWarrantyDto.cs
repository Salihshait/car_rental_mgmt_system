namespace CarRent.Application.DTOs.Maintenance;

public class VehicleWarrantyDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string WarrantyType { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateVehicleWarrantyRequest
{
    public Guid VehicleId { get; set; }
    public string WarrantyType { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
}
