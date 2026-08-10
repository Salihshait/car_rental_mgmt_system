using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IFraudDetectionService
{
    Task<FraudAlertDto?> EvaluateBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status, CancellationToken cancellationToken = default);
    Task<FraudAlertDto> ReviewAlertAsync(Guid alertId, Guid reviewerId, ReviewFraudAlertRequest request, CancellationToken cancellationToken = default);
}
