using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface IPlanLimitService
{
    Task<IEnumerable<EffectiveLimitDto>> GetEffectiveLimitsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> CheckLimitAsync(Guid tenantId, string limitKey, int currentUsage, CancellationToken cancellationToken = default);
}
