namespace CarRent.Application.DTOs.Insurance;

public class CreateInsuranceRequest
{
    public Guid VehicleId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public string Provider { get; set; } = string.Empty;
}
