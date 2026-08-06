namespace CarRent.Domain.Entities;

public class DriverAssignment
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
    public Guid AssignedBy { get; set; }
    public string? Notes { get; set; }
}
