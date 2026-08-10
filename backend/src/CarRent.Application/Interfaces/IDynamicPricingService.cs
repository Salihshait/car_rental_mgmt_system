using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IDynamicPricingService
{
    Task<DynamicPricingResultDto> GetSuggestedPriceAsync(Guid vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
