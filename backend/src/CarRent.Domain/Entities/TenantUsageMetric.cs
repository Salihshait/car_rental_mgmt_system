namespace CarRent.Domain.Entities;

public class TenantUsageMetric
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string MetricKey { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
