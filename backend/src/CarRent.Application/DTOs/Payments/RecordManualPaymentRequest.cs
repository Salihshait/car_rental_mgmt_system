namespace CarRent.Application.DTOs.Payments;

public class RecordManualPaymentRequest
{
    public Guid BookingId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? TransactionReference { get; set; }
}
