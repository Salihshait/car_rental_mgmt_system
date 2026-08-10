using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IRecommendationEngineService
{
    Task<IEnumerable<VehicleRecommendationDto>> GetRecommendationsAsync(Guid customerUserId, CancellationToken cancellationToken = default);
}
