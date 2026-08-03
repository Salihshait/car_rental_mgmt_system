namespace CarRent.Domain.Entities;

public class Insurance
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string Provider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Vehicle Vehicle { get; set; } = default!;
}
