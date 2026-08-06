namespace CarRent.Application.DTOs.Coupons;

public class ValidateCouponResultDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
}
