using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No real provider configured yet. Logs every outgoing SMS so booking flows can be exercised end-to-end.
/// Swap for a real Twilio implementation by registering a different ISmsService in Program.cs.
/// </summary>
public class LoggingSmsService : ISmsService
{
    private readonly ILogger<LoggingSmsService> _logger;

    public LoggingSmsService(ILogger<LoggingSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SmsStub] To: {ToPhoneNumber} | Message: {Message}", toPhoneNumber, message);
        return Task.CompletedTask;
    }
}
