namespace CarRent.Application.DTOs.Payments;

public class CreatePaymentRequest
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Card";
    public string Gateway { get; set; } = "Razorpay";
    public string Status { get; set; } = "Verified";
    public string? TransactionReference { get; set; }
    public DateTime? PaidAt { get; set; }
}
