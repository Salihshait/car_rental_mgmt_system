namespace CarRent.Application.DTOs.Customers;

public class UpdateEmergencyContactRequest
{
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
}
