namespace CarRent.Application.DTOs.Invoices;

public class CreateInvoiceRequest
{
    public Guid BookingId { get; set; }
    public decimal? ManualDiscountAmount { get; set; }
    public string? ManualDiscountReason { get; set; }
}
