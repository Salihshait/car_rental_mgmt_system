namespace CarRent.Application.DTOs.Billing;

public class RefundDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string RefundMethod { get; set; } = string.Empty;
    public string? Gateway { get; set; }
    public string? GatewayRefundReference { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreateRefundRequest
{
    public Guid BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string RefundMethod { get; set; } = "Original";
}

public class RejectRefundRequest
{
    public string? Reason { get; set; }
}
