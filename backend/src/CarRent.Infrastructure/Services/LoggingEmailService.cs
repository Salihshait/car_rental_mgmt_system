using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No real provider configured yet. Logs every outgoing email so booking flows can be exercised end-to-end.
/// Swap for a real SMTP/SendGrid implementation by registering a different IEmailService in Program.cs.
/// </summary>
public class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EmailStub] To: {ToEmail} | Subject: {Subject} | Body: {Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
