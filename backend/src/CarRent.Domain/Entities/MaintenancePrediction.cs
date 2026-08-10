namespace CarRent.Domain.Entities;

public class MaintenancePrediction
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string PredictedIssue { get; set; } = string.Empty;
    public DateTime PredictedDueDate { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string BasisSummary { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
