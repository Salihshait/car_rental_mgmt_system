namespace CarRent.Application.DTOs.Saas;

public record TenantDto(Guid Id, string CompanyName, string Slug, string ContactEmail, string? ContactPhone, string Status, DateTime? TrialEndsAt, DateTime CreatedAt);

public record RegisterTenantRequest(string CompanyName, string Slug, string ContactEmail, string? ContactPhone);

public record UpdateTenantRequest(string CompanyName, string ContactEmail, string? ContactPhone, string Status);

public record TenantDatabaseInfoDto(string IsolationModel, int SubscriptionCount, int InvoiceCount, int UsageMetricRecordCount);
