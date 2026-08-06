namespace CarRent.Application.DTOs.Fleet;

public class DriverAssignmentDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid DriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public string? Notes { get; set; }
}

public class AssignDriverRequest
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public string? Notes { get; set; }
}
