namespace CarRent.Domain.Entities;

public class TenantDomain
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
