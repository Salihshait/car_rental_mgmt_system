using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IDamageDetectionService
{
    Task<DamageDetectionResultDto> AnalyzeAsync(Guid? vehicleId, Guid? rentalId, string imageReference, byte[] imageBytes, CancellationToken cancellationToken = default);
    Task<IEnumerable<DamageDetectionResultDto>> GetHistoryAsync(Guid? vehicleId, CancellationToken cancellationToken = default);
}
