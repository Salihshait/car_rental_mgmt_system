namespace CarRent.Domain.Entities;

public class DamageDetectionResult
{
    public Guid Id { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? RentalId { get; set; }
    public string ImageReference { get; set; } = string.Empty;
    public string DetectedDamagesJson { get; set; } = "[]";
    public decimal SeverityScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
