using CarRent.Application.DTOs.Billing;

namespace CarRent.Application.Interfaces;

public interface IRefundService
{
    Task<IEnumerable<RefundDto>> GetAllAsync(Guid? bookingId, CancellationToken cancellationToken = default);
    Task<RefundDto> CreateAsync(CreateRefundRequest request, Guid requestedBy, CancellationToken cancellationToken = default);
    Task<RefundDto> ApproveAsync(Guid id, Guid processedBy, CancellationToken cancellationToken = default);
    Task<RefundDto> RejectAsync(Guid id, RejectRefundRequest request, Guid processedBy, CancellationToken cancellationToken = default);
}
