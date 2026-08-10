namespace CarRent.Domain.Entities;

public class FraudAlert
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid CustomerId { get; set; }
    public int RiskScore { get; set; }
    public string Reasons { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
