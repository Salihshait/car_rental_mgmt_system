namespace CarRent.Domain.Entities;

public class Complaint
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? VehicleId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public string? Resolution { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
