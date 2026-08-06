using CarRent.Application.DTOs.Bookings;

namespace CarRent.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingSummaryDto>> GetAllAsync(BookingFilter filter, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<BookingDto?> GetByIdAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IEnumerable<BookingTimelineEntryDto>> GetTimelineAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);

    Task<PricingBreakdownDto> QuoteAsync(BookingQuoteRequest request, CancellationToken cancellationToken = default);

    Task<BookingDto> CreateOnlineAsync(Guid customerId, CreateOnlineBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingDto> CreateWalkInAsync(Guid staffUserId, CreateWalkInBookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDto> UpdateAsync(Guid id, UpdateBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<BookingDto> CancelAsync(Guid id, CancelBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<BookingDto> ApproveAsync(Guid id, Guid approverUserId, CancellationToken cancellationToken = default);
    Task<BookingDto> RejectAsync(Guid id, RejectBookingRequest request, Guid approverUserId, CancellationToken cancellationToken = default);
    Task<BookingDto> ExtendAsync(Guid id, ExtendBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);

    Task<BookingReportSummaryDto> GetReportSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
}
