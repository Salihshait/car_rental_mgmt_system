namespace CarRent.Application.DTOs.Customers;

public class WalletAdjustRequest
{
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
