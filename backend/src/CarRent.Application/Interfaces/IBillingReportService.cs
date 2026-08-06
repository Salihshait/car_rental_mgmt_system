using CarRent.Application.DTOs.Billing;

namespace CarRent.Application.Interfaces;

public interface IBillingReportService
{
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutstandingInvoiceDto>> GetOutstandingInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethodBreakdownDto>> GetPaymentMethodBreakdownAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
