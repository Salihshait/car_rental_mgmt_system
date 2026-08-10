namespace CarRent.Application.DTOs.Saas;

public record PlanLimitDto(Guid Id, string LimitKey, int LimitValue);

public record PlanFeatureDto(Guid Id, string FeatureKey, bool IsEnabled);

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    string Currency,
    bool IsActive,
    List<PlanLimitDto> Limits,
    List<PlanFeatureDto> Features,
    DateTime CreatedAt);

public record LimitInput(string LimitKey, int LimitValue);

public record FeatureInput(string FeatureKey, bool IsEnabled);

public record UpsertPlanRequest(
    string Name,
    string? Description,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    string Currency,
    bool IsActive,
    List<LimitInput> Limits,
    List<FeatureInput> Features);
