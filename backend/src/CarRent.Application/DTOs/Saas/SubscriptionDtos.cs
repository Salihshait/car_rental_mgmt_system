namespace CarRent.Application.DTOs.Saas;

public record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    Guid PlanId,
    string? PlanName,
    string Status,
    string BillingCycle,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    DateTime CreatedAt);

public record CreateSubscriptionRequest(Guid PlanId, string BillingCycle);

public record EffectiveLimitDto(string LimitKey, int LimitValue, string? PlanName);
