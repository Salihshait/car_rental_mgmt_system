using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class CampaignService : ICampaignService
{
    private static readonly string[] DeletableStatuses = { "Draft" };
    private static readonly string[] CancellableStatuses = { "Draft", "Scheduled" };

    private readonly CarRentDbContext _context;

    public CampaignService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CampaignDto>> GetAllAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }

        var campaigns = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
        return await ToDtosAsync(campaigns, cancellationToken);
    }

    public async Task<CampaignDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Campaign not found.");
        return await ToDtoAsync(campaign, cancellationToken);
    }

    public async Task<CampaignDto> CreateAsync(Guid createdByUserId, CreateCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        var audience = await CampaignAudienceResolver.ResolveAsync(_context, request.AudienceFilter, cancellationToken);

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TemplateId = request.TemplateId,
            Channel = template.Channel,
            AudienceFilter = request.AudienceFilter,
            Status = "Draft",
            TargetCount = audience.Count,
            CreatedBy = createdByUserId
        };

        await _context.Campaigns.AddAsync(campaign, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(campaign, cancellationToken);
    }

    public async Task<CampaignDto> ScheduleAsync(Guid id, ScheduleCampaignRequest request, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Campaign not found.");

        if (campaign.Status != "Draft")
        {
            throw new InvalidOperationException("Only draft campaigns can be scheduled.");
        }

        campaign.Status = "Scheduled";
        campaign.ScheduledAt = request.ScheduledAt;
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(campaign, cancellationToken);
    }

    public async Task<CampaignDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Campaign not found.");

        if (!CancellableStatuses.Contains(campaign.Status))
        {
            throw new InvalidOperationException($"Cannot cancel a campaign with status '{campaign.Status}'.");
        }

        campaign.Status = "Cancelled";
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(campaign, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Campaign not found.");

        if (!DeletableStatuses.Contains(campaign.Status))
        {
            throw new InvalidOperationException("Only draft campaigns can be deleted.");
        }

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<MessageLogDto>> GetLogsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var logs = await _context.MessageLogs.AsNoTracking()
            .Where(l => l.CampaignId == id)
            .OrderByDescending(l => l.SentAt)
            .ToListAsync(cancellationToken);

        return logs.Select(l => new MessageLogDto(l.Id, l.Channel, l.RecipientUserId, l.RecipientAddress, l.TemplateId, l.CampaignId, l.Subject, l.Body, l.Status, l.ErrorMessage, l.SentAt));
    }

    private async Task<CampaignDto> ToDtoAsync(Campaign campaign, CancellationToken cancellationToken)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == campaign.TemplateId, cancellationToken);
        return MapDto(campaign, template);
    }

    private async Task<List<CampaignDto>> ToDtosAsync(List<Campaign> campaigns, CancellationToken cancellationToken)
    {
        var templateIds = campaigns.Select(c => c.TemplateId).Distinct().ToList();
        var templates = await _context.MessageTemplates.AsNoTracking().Where(t => templateIds.Contains(t.Id)).ToListAsync(cancellationToken);
        return campaigns.Select(c => MapDto(c, templates.FirstOrDefault(t => t.Id == c.TemplateId))).ToList();
    }

    private static CampaignDto MapDto(Campaign campaign, MessageTemplate? template) => new(
        campaign.Id,
        campaign.Name,
        campaign.TemplateId,
        template?.Name,
        campaign.Channel,
        campaign.AudienceFilter,
        campaign.Status,
        campaign.ScheduledAt,
        campaign.StartedAt,
        campaign.CompletedAt,
        campaign.TargetCount,
        campaign.SentCount,
        campaign.FailedCount,
        campaign.CreatedAt);
}
