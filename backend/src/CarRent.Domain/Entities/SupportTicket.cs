namespace CarRent.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Priority { get; set; } = "Normal";
    public string Status { get; set; } = "Open";
    public Guid? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
