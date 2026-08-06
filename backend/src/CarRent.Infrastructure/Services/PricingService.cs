using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces;

namespace CarRent.Infrastructure.Services;

public class PricingService : IPricingService
{
    private const string TaxRateSettingKey = "TaxRatePercent";
    private const decimal DefaultTaxRatePercent = 8m;

    private readonly ISettingsService _settingsService;
    private readonly ICouponService _couponService;

    public PricingService(ISettingsService settingsService, ICouponService couponService)
    {
        _settingsService = settingsService;
        _couponService = couponService;
    }

    public async Task<PricingBreakdownDto> CalculateAsync(decimal dailyRate, DateTime startDate, DateTime endDate, string? couponCode, CancellationToken cancellationToken = default)
    {
        if (endDate <= startDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        var days = Math.Max(1, (int)Math.Ceiling((endDate - startDate).TotalDays));
        var subtotal = Math.Round(dailyRate * days, 2);

        decimal discount = 0;
        string? appliedCouponCode = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var validation = await _couponService.ValidateAsync(couponCode, subtotal, cancellationToken);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.Message ?? "Invalid coupon code.");
            }

            discount = validation.DiscountAmount;
            appliedCouponCode = couponCode.Trim().ToUpperInvariant();
        }

        var taxRateRaw = await _settingsService.GetAsync(TaxRateSettingKey, cancellationToken);
        var taxRate = decimal.TryParse(taxRateRaw, out var parsedRate) ? parsedRate : DefaultTaxRatePercent;

        var taxableAmount = subtotal - discount;
        var tax = Math.Round(taxableAmount * taxRate / 100m, 2);
        var total = taxableAmount + tax;

        return new PricingBreakdownDto
        {
            Days = days,
            DailyRate = dailyRate,
            SubtotalAmount = subtotal,
            CouponCode = appliedCouponCode,
            DiscountAmount = discount,
            TaxRatePercent = taxRate,
            TaxAmount = tax,
            TotalAmount = total
        };
    }
}
