namespace CarRent.Application.DTOs.Payments;

public class InitiatePaymentRequest
{
    public Guid BookingId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Gateway { get; set; } = "Razorpay";
    public string Currency { get; set; } = "INR";
}

public class PaymentOrderDto
{
    public Guid PaymentId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
