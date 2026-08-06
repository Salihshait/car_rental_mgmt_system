namespace CarRent.Application.DTOs.Bookings;

public class PricingBreakdownDto
{
    public int Days { get; set; }
    public decimal DailyRate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRatePercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
