using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface IFeatureToggleService
{
    Task<IEnumerable<TenantFeatureOverrideDto>> GetTenantOverridesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantFeatureOverrideDto> UpsertTenantOverrideAsync(Guid tenantId, UpsertTenantFeatureOverrideRequest request, CancellationToken cancellationToken = default);
    Task<FeatureResolutionDto> ResolveAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default);
}
