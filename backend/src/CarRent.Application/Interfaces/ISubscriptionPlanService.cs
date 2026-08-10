using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface ISubscriptionPlanService
{
    Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanDto> CreateAsync(UpsertPlanRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionPlanDto> UpdateAsync(Guid id, UpsertPlanRequest request, CancellationToken cancellationToken = default);
}
