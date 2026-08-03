using CarRent.Application.DTOs.Payments;

namespace CarRent.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaymentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentSummaryDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
