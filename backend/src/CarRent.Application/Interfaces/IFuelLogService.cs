using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IFuelLogService
{
    Task<IEnumerable<FuelLogDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<FuelLogDto> CreateAsync(CreateFuelLogRequest request, Guid recordedBy, CancellationToken cancellationToken = default);
    Task<FuelConsumptionSummaryDto> GetConsumptionSummaryAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
