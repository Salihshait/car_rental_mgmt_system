namespace CarRent.Application.DTOs.Customers;

public class LoyaltyAdjustRequest
{
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}
