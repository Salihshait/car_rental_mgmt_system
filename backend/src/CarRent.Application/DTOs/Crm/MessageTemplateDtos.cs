namespace CarRent.Application.DTOs.Crm;

public record MessageTemplateDto(Guid Id, string Name, string Channel, string? Subject, string Body, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);

public record UpsertTemplateRequest(string Name, string Channel, string? Subject, string Body, bool IsActive);

public record TemplatePreviewRequest(Dictionary<string, string>? SampleValues);

public record TemplatePreviewResult(string? Subject, string Body);
