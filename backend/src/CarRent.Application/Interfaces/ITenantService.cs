using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface ITenantService
{
    Task<TenantDto> RegisterAsync(RegisterTenantRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TenantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<TenantDatabaseInfoDto> GetDatabaseInfoAsync(Guid id, CancellationToken cancellationToken = default);
}
