namespace CarRent.Application.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public int LoyaltyPoints { get; set; }
    public bool IsBlacklisted { get; set; }
    public bool IsCorporate { get; set; }
    public string? CompanyName { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public DateTime CreatedAt { get; set; }
}
