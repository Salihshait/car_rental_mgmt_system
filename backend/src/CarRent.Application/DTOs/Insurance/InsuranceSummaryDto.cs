namespace CarRent.Application.DTOs.Insurance;

public class InsuranceSummaryDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string Provider { get; set; } = string.Empty;
}
