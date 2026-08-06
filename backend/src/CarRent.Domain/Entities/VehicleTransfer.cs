namespace CarRent.Domain.Entities;

public class VehicleTransfer
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid FromBranchId { get; set; }
    public Guid ToBranchId { get; set; }
    public Guid RequestedBy { get; set; }
    public string Status { get; set; } = "InTransit";
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}
