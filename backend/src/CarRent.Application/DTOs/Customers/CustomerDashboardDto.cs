namespace CarRent.Application.DTOs.Customers;

public class CustomerDashboardDto
{
    public int TotalCustomers { get; set; }
    public int BlacklistedCount { get; set; }
    public int CorporateCount { get; set; }
    public decimal TotalWalletBalance { get; set; }
    public int TotalLoyaltyPoints { get; set; }
    public Dictionary<string, int> KycStatusCounts { get; set; } = new();
}
