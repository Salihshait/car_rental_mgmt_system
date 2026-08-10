namespace CarRent.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Status { get; set; } = "Trial";
    public DateTime? TrialEndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
