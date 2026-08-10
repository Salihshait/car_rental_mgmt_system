using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IVoiceBookingService
{
    Task<VoiceBookingRequestDto> SubmitAsync(Guid customerId, byte[] audioBytes, CancellationToken cancellationToken = default);
    Task<IEnumerable<VoiceBookingRequestDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
