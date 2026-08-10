using CarRent.Application.DTOs.Saas;

namespace CarRent.Application.Interfaces;

public interface IBillingService
{
    Task<SubscriptionInvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubscriptionInvoiceDto>> GetAllAsync(Guid? tenantId, string? status, CancellationToken cancellationToken = default);
    Task<SubscriptionInvoiceDto> MarkPaidAsync(Guid invoiceId, MarkInvoicePaidRequest request, CancellationToken cancellationToken = default);
}
