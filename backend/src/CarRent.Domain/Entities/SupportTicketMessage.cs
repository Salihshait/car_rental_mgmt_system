namespace CarRent.Domain.Entities;

public class SupportTicketMessage
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid SenderUserId { get; set; }
    public bool IsInternalNote { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
