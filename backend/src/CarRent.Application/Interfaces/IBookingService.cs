using CarRent.Application.DTOs.Bookings;

namespace CarRent.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BookingSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BookingSummaryDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
}
