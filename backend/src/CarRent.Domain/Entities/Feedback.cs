namespace CarRent.Domain.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string Category { get; set; } = "General";
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
