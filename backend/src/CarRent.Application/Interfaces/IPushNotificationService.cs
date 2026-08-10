namespace CarRent.Application.Interfaces;

public interface IPushNotificationService
{
    Task SendAsync(string recipient, string message, CancellationToken cancellationToken = default);
}
