using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IRevenuePredictionService
{
    Task<ForecastDto> GetForecastAsync(int monthsAhead, CancellationToken cancellationToken = default);
}
