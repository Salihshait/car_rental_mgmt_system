using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface ITenantBrandingService
{
    Task<TenantBrandingDto> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantBrandingDto> UpsertAsync(Guid tenantId, UpsertTenantBrandingRequest request, CancellationToken cancellationToken = default);
}
