namespace CarRent.Application.DTOs.Customers;

public class UpdateCustomerRequest
{
    public string KycStatus { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public bool IsCorporate { get; set; }
    public string? CompanyName { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}
