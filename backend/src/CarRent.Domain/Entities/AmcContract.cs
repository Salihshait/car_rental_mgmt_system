namespace CarRent.Domain.Entities;

public class AmcContract
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid VendorId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
