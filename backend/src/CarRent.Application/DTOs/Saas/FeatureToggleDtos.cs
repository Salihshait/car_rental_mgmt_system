namespace CarRent.Application.DTOs.Saas;

public record FeatureResolutionDto(string FeatureKey, bool IsEnabled, string Source);

public record UpsertPlanFeatureRequest(string FeatureKey, bool IsEnabled);

public record UpsertTenantFeatureOverrideRequest(string FeatureKey, bool IsEnabled);

public record TenantFeatureOverrideDto(Guid Id, string FeatureKey, bool IsEnabled);
