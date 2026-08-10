namespace CarRent.Application.DTOs.Saas;

public record SubscriptionInvoiceDto(
    Guid Id,
    Guid TenantId,
    string? TenantName,
    Guid SubscriptionId,
    string InvoiceNumber,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? PaidAt,
    string? GatewayReference,
    DateTime CreatedAt);

public record GenerateInvoiceRequest(Guid SubscriptionId);

public record MarkInvoicePaidRequest(string Gateway);
