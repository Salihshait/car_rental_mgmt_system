namespace CarRent.Domain.Entities;

public class DriverAttendance
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string Status { get; set; } = "Present";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
