namespace CarRent.Domain.Entities;

public class VehicleWarranty
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string WarrantyType { get; set; } = string.Empty;
    public string? Provider { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? CoverageDetails { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
