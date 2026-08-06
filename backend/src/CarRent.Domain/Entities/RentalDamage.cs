namespace CarRent.Domain.Entities;

public class RentalDamage
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Stage { get; set; } = "Pickup";
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Minor";
    public decimal EstimatedRepairCost { get; set; }
    public Guid ReportedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
