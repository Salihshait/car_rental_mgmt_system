using CarRent.Application.DTOs.Invoices;

namespace CarRent.Application.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceSummaryDto> GenerateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
