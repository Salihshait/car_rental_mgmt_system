namespace CarRent.Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
