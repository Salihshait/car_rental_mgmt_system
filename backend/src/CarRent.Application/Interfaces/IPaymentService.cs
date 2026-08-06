using CarRent.Application.DTOs.Payments;

namespace CarRent.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentSummaryDto>> GetAllAsync(Guid? bookingId, Guid? invoiceId, CancellationToken cancellationToken = default);
    Task<PaymentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PaymentOrderDto> InitiateAsync(InitiatePaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentSummaryDto> ConfirmAsync(Guid paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentSummaryDto> RecordManualPaymentAsync(RecordManualPaymentRequest request, Guid recordedBy, CancellationToken cancellationToken = default);

    /// <summary>Sums this invoice's Verified payments and updates AmountPaid/Status. Also used by refunds after money moves back out.</summary>
    Task RecalculateInvoiceStatusAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
