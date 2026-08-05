namespace CarRent.Application.DTOs.Customers;

public class CustomerFilter
{
    public string? Search { get; set; }
    public string? KycStatus { get; set; }
    public bool? IsBlacklisted { get; set; }
    public bool? IsCorporate { get; set; }
}
