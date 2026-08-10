namespace CarRent.Application.DTOs.Crm;

public record MessageLogDto(
    Guid Id,
    string Channel,
    Guid? RecipientUserId,
    string RecipientAddress,
    Guid? TemplateId,
    Guid? CampaignId,
    string? Subject,
    string Body,
    string Status,
    string? ErrorMessage,
    DateTime SentAt);

public record SendAdHocMessageRequest(
    string Channel,
    string RecipientAddress,
    Guid? RecipientUserId,
    Guid? TemplateId,
    string? Subject,
    string? Body,
    Dictionary<string, string>? PlaceholderValues);
