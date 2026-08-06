namespace CarRent.Application.DTOs.Maintenance;

public class AmcContractDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateAmcContractRequest
{
    public Guid VehicleId { get; set; }
    public Guid VendorId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
    public decimal Cost { get; set; }
}
