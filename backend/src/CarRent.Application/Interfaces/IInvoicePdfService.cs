using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces;

public interface IInvoicePdfService
{
    byte[] Generate(Invoice invoice, IReadOnlyList<InvoiceLineItem> lineItems, Booking booking, Vehicle vehicle, User customer, Branch? branch);
}
