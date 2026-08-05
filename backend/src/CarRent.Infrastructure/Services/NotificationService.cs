using CarRent.Application.DTOs.Notifications;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly CarRentDbContext _context;

    public NotificationService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                NotificationType = n.NotificationType,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Notification not found.");

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
