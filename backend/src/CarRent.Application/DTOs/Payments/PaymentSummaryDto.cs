namespace CarRent.Application.DTOs.Payments;

public class PaymentSummaryDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? GatewayOrderId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
