using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IDemandPredictionService
{
    Task<ForecastDto> GetForecastAsync(Guid? branchId, Guid? categoryId, int monthsAhead, CancellationToken cancellationToken = default);
}
