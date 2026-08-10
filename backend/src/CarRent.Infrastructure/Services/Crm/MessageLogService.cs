using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class MessageLogService : IMessageLogService
{
    private readonly CarRentDbContext _context;
    private readonly IMessageDispatchService _dispatchService;

    public MessageLogService(CarRentDbContext context, IMessageDispatchService dispatchService)
    {
        _context = context;
        _dispatchService = dispatchService;
    }

    public async Task<IEnumerable<MessageLogDto>> GetAllAsync(string? channel, string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.MessageLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(l => l.Channel == channel);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(l => l.Status == status);
        }
        if (from.HasValue)
        {
            query = query.Where(l => l.SentAt >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(l => l.SentAt <= to.Value);
        }

        var logs = await query.OrderByDescending(l => l.SentAt).Take(500).ToListAsync(cancellationToken);
        return logs.Select(l => new MessageLogDto(l.Id, l.Channel, l.RecipientUserId, l.RecipientAddress, l.TemplateId, l.CampaignId, l.Subject, l.Body, l.Status, l.ErrorMessage, l.SentAt));
    }

    public async Task<MessageLogDto> SendAdHocAsync(SendAdHocMessageRequest request, CancellationToken cancellationToken = default)
    {
        string? subject = request.Subject;
        string body;
        Guid? templateId = request.TemplateId;

        if (request.TemplateId.HasValue)
        {
            var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken)
                ?? throw new InvalidOperationException("Template not found.");

            var values = request.PlaceholderValues ?? new Dictionary<string, string>();
            body = TemplateRenderer.Render(template.Body, values);
            subject = template.Subject is null ? null : TemplateRenderer.Render(template.Subject, values);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                throw new InvalidOperationException("Either a template or a message body is required.");
            }
            body = request.Body;
        }

        var logId = await _dispatchService.DispatchAsync(request.Channel, request.RecipientAddress, request.RecipientUserId, subject, body, templateId, null, cancellationToken);
        var log = await _context.MessageLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == logId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to record the message log.");

        return new MessageLogDto(log.Id, log.Channel, log.RecipientUserId, log.RecipientAddress, log.TemplateId, log.CampaignId, log.Subject, log.Body, log.Status, log.ErrorMessage, log.SentAt);
    }
}
