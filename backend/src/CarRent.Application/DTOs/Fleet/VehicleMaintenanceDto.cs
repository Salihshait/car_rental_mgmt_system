namespace CarRent.Application.DTOs.Fleet;

public class VehicleMaintenanceDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid? WorkshopId { get; set; }
    public string? WorkshopName { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ScheduledOn { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Cost { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMaintenanceRequest
{
    public Guid VehicleId { get; set; }
    public Guid? WorkshopId { get; set; }
    public string MaintenanceType { get; set; } = "Scheduled";
    public string ServiceType { get; set; } = string.Empty;
    public DateTime ScheduledOn { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
}

public class CompleteMaintenanceRequest
{
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
}
