namespace CarRent.Domain.Entities;

public class VehicleMaintenance
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? WorkshopId { get; set; }
    public string MaintenanceType { get; set; } = "Scheduled";
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ScheduledOn { get; set; }
    public string Status { get; set; } = "Scheduled";
    public decimal? Cost { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
