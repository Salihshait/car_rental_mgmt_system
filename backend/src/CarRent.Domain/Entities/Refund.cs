namespace CarRent.Domain.Entities;

public class Refund
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string RefundMethod { get; set; } = "Original";
    public string? Gateway { get; set; }
    public string? GatewayRefundReference { get; set; }
    public string Status { get; set; } = "Requested";
    public Guid? RequestedBy { get; set; }
    public Guid? ProcessedBy { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
