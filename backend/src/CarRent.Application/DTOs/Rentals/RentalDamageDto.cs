namespace CarRent.Application.DTOs.Rentals;

public class RentalDamageDto
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal EstimatedRepairCost { get; set; }
    public Guid ReportedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRentalDamageRequest
{
    public string Stage { get; set; } = "Pickup";
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Minor";
    public decimal EstimatedRepairCost { get; set; }
}
