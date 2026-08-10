using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No real push provider (Firebase/OneSignal) configured yet, and no device-token storage exists
/// in this schema. Logs every outgoing push against the recipient's user id/email as a stand-in
/// so CRM flows can be exercised end-to-end. Swap for a real implementation by registering a
/// different IPushNotificationService in Program.cs.
/// </summary>
public class LoggingPushNotificationService : IPushNotificationService
{
    private readonly ILogger<LoggingPushNotificationService> _logger;

    public LoggingPushNotificationService(ILogger<LoggingPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[PushStub] To: {Recipient} | Message: {Message}", recipient, message);
        return Task.CompletedTask;
    }
}
