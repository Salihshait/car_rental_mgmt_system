namespace CarRent.Application.DTOs.Drivers;

public class DriverAttendanceDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class MarkAttendanceRequest
{
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = "Absent";
    public string? Notes { get; set; }
}
