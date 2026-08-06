namespace CarRent.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string KycStatus { get; set; } = "Pending";
    public decimal WalletBalance { get; set; }
    public int LoyaltyPoints { get; set; }
    public bool IsBlacklisted { get; set; }
    public bool IsCorporate { get; set; }
    public string? CompanyName { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? Gstin { get; set; }
    public string? BillingState { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}
