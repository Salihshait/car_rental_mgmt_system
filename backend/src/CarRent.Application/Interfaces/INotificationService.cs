using CarRent.Application.DTOs.Notifications;

namespace CarRent.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task CreateAsync(Guid userId, string notificationType, string message, CancellationToken cancellationToken = default);
}
