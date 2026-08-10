namespace CarRent.Domain.Entities;

public class SubscriptionInvoice
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
    public string? GatewayReference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
