using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IPredictiveMaintenanceService
{
    Task GeneratePredictionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenancePredictionDto>> GetPredictionsAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default);
    Task<MaintenancePredictionDto> UpdateStatusAsync(Guid id, UpdatePredictionStatusRequest request, CancellationToken cancellationToken = default);
}
