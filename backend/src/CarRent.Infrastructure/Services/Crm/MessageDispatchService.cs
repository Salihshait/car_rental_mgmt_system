using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Crm;

public class MessageDispatchService : IMessageDispatchService
{
    private readonly CarRentDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<MessageDispatchService> _logger;

    public MessageDispatchService(
        CarRentDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        IPushNotificationService pushNotificationService,
        ILogger<MessageDispatchService> logger)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    public async Task<Guid> DispatchAsync(
        string channel,
        string recipientAddress,
        Guid? recipientUserId,
        string? subject,
        string body,
        Guid? templateId,
        Guid? campaignId,
        CancellationToken cancellationToken = default)
    {
        var log = new MessageLog
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            RecipientUserId = recipientUserId,
            RecipientAddress = recipientAddress,
            TemplateId = templateId,
            CampaignId = campaignId,
            Subject = subject,
            Body = body
        };

        try
        {
            await SendAsync(channel, recipientAddress, subject, body, cancellationToken);
            log.Status = "Simulated";
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "Failed to dispatch {Channel} message to {Recipient}", channel, recipientAddress);
        }

        await _context.MessageLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return log.Id;
    }

    private Task SendAsync(string channel, string recipientAddress, string? subject, string body, CancellationToken cancellationToken) => channel switch
    {
        "Email" => _emailService.SendAsync(recipientAddress, subject ?? string.Empty, body, cancellationToken),
        "Sms" => _smsService.SendAsync(recipientAddress, body, cancellationToken),
        "WhatsApp" => _whatsAppService.SendAsync(recipientAddress, body, cancellationToken),
        "Push" => _pushNotificationService.SendAsync(recipientAddress, body, cancellationToken),
        _ => throw new InvalidOperationException($"'{channel}' is not a supported message channel.")
    };
}
