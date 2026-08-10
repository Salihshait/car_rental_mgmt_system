namespace CarRent.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public string Channel { get; set; } = "Web";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
}
