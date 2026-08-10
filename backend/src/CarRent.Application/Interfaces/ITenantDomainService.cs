using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface ITenantDomainService
{
    Task<IEnumerable<TenantDomainDto>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantDomainDto> CreateAsync(Guid tenantId, CreateTenantDomainRequest request, CancellationToken cancellationToken = default);
    Task<TenantDomainDto> VerifyAsync(Guid domainId, CancellationToken cancellationToken = default);
}
