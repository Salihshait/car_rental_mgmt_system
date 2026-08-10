using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No real WhatsApp Business API account configured yet. Logs every outgoing message so CRM
/// flows can be exercised end-to-end. Swap for a real implementation by registering a different
/// IWhatsAppService in Program.cs.
/// </summary>
public class LoggingWhatsAppService : IWhatsAppService
{
    private readonly ILogger<LoggingWhatsAppService> _logger;

    public LoggingWhatsAppService(ILogger<LoggingWhatsAppService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[WhatsAppStub] To: {ToPhoneNumber} | Message: {Message}", toPhoneNumber, message);
        return Task.CompletedTask;
    }
}
