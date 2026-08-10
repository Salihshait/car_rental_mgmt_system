using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Crm;

public class CampaignExecutionService : ICampaignExecutionService
{
    private static readonly string[] SendableStatuses = { "Draft", "Scheduled" };

    private readonly CarRentDbContext _context;
    private readonly IMessageDispatchService _dispatchService;
    private readonly ILogger<CampaignExecutionService> _logger;

    public CampaignExecutionService(CarRentDbContext context, IMessageDispatchService dispatchService, ILogger<CampaignExecutionService> logger)
    {
        _context = context;
        _dispatchService = dispatchService;
        _logger = logger;
    }

    public async Task SendNowAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken)
            ?? throw new InvalidOperationException("Campaign not found.");

        if (!SendableStatuses.Contains(campaign.Status))
        {
            throw new InvalidOperationException($"Cannot send a campaign with status '{campaign.Status}'.");
        }

        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == campaign.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        var audience = await CampaignAudienceResolver.ResolveAsync(_context, campaign.AudienceFilter, cancellationToken);

        campaign.Status = "Sending";
        campaign.StartedAt = DateTime.UtcNow;
        campaign.TargetCount = audience.Count;
        campaign.SentCount = 0;
        campaign.FailedCount = 0;
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var (user, customer) in audience)
        {
            var placeholders = new Dictionary<string, string>
            {
                ["CustomerName"] = $"{user.FirstName} {user.LastName}".Trim(),
                ["CompanyName"] = customer.CompanyName ?? string.Empty,
            };

            var recipientAddress = campaign.Channel switch
            {
                "Email" => user.Email,
                "Sms" or "WhatsApp" => user.PhoneNumber ?? string.Empty,
                "Push" => user.Email,
                _ => user.Email
            };

            if (string.IsNullOrWhiteSpace(recipientAddress))
            {
                campaign.FailedCount++;
                continue;
            }

            var body = TemplateRenderer.Render(template.Body, placeholders);
            var subject = template.Subject is null ? null : TemplateRenderer.Render(template.Subject, placeholders);

            var logId = await _dispatchService.DispatchAsync(campaign.Channel, recipientAddress, user.Id, subject, body, template.Id, campaign.Id, cancellationToken);
            var log = await _context.MessageLogs.AsNoTracking().FirstOrDefaultAsync(l => l.Id == logId, cancellationToken);

            if (log?.Status == "Failed")
            {
                campaign.FailedCount++;
            }
            else
            {
                campaign.SentCount++;
            }
        }

        campaign.Status = "Completed";
        campaign.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} completed: {Sent} sent, {Failed} failed", campaign.Id, campaign.SentCount, campaign.FailedCount);
    }
}
