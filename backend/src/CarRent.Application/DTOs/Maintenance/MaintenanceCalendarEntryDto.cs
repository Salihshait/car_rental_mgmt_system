namespace CarRent.Application.DTOs.Maintenance;

public class MaintenanceCalendarEntryDto
{
    public string Type { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string? Status { get; set; }
}
