using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateAsync(Guid tenantId, CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionDto>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto> CancelAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
