namespace CarRent.Application.DTOs.Crm;

public record CampaignDto(
    Guid Id,
    string Name,
    Guid TemplateId,
    string? TemplateName,
    string Channel,
    string AudienceFilter,
    string Status,
    DateTime? ScheduledAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int TargetCount,
    int SentCount,
    int FailedCount,
    DateTime CreatedAt);

public record CreateCampaignRequest(string Name, Guid TemplateId, string AudienceFilter);

public record ScheduleCampaignRequest(DateTime ScheduledAt);
