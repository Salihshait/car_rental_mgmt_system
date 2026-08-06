using CarRent.Application.DTOs.Coupons;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRent.UnitTests;

public class CouponAndPricingServiceTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    [Fact]
    public async Task ValidateAsync_RejectsExpiredCoupon()
    {
        await using var context = CreateContext();
        var couponService = new CouponService(context);
        await couponService.CreateAsync(new CreateCouponRequest
        {
            Code = "OLD10",
            DiscountType = "Percentage",
            DiscountValue = 10,
            EndDate = DateTime.UtcNow.AddDays(-1)
        });

        var result = await couponService.ValidateAsync("OLD10", 500);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_RejectsBelowMinimumBookingAmount()
    {
        await using var context = CreateContext();
        var couponService = new CouponService(context);
        await couponService.CreateAsync(new CreateCouponRequest
        {
            Code = "MIN200",
            DiscountType = "Flat",
            DiscountValue = 20,
            MinBookingAmount = 200
        });

        var result = await couponService.ValidateAsync("MIN200", 150);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_RejectsWhenUsageLimitReached()
    {
        await using var context = CreateContext();
        var couponService = new CouponService(context);
        var created = await couponService.CreateAsync(new CreateCouponRequest
        {
            Code = "ONCE",
            DiscountType = "Flat",
            DiscountValue = 15,
            UsageLimit = 1
        });

        await couponService.RedeemAsync(created.Id, Guid.NewGuid(), Guid.NewGuid(), 15);

        var result = await couponService.ValidateAsync("ONCE", 100);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_CapsPercentageDiscountAtMaxDiscountAmount()
    {
        await using var context = CreateContext();
        var couponService = new CouponService(context);
        await couponService.CreateAsync(new CreateCouponRequest
        {
            Code = "SUMMER50",
            DiscountType = "Percentage",
            DiscountValue = 50,
            MaxDiscountAmount = 40
        });

        // 50% of 200 would be 100, but the cap should limit it to 40.
        var result = await couponService.ValidateAsync("SUMMER50", 200);

        Assert.True(result.IsValid);
        Assert.Equal(40, result.DiscountAmount);
    }

    [Fact]
    public async Task CalculateAsync_AppliesConfiguredTaxRate_WhenNoCoupon()
    {
        await using var context = CreateContext();
        var settingsService = new SettingsService(context);
        var couponService = new CouponService(context);
        await settingsService.SetAsync("TaxRatePercent", "10", "Billing");

        var pricingService = new PricingService(settingsService, couponService);
        var pricing = await pricingService.CalculateAsync(dailyRate: 100, startDate: new DateTime(2026, 1, 1), endDate: new DateTime(2026, 1, 4), couponCode: null);

        Assert.Equal(3, pricing.Days);
        Assert.Equal(300, pricing.SubtotalAmount);
        Assert.Equal(0, pricing.DiscountAmount);
        Assert.Equal(30, pricing.TaxAmount);
        Assert.Equal(330, pricing.TotalAmount);
    }

    [Fact]
    public async Task CalculateAsync_AppliesDiscountBeforeTax_WhenCouponProvided()
    {
        await using var context = CreateContext();
        var settingsService = new SettingsService(context);
        var couponService = new CouponService(context);
        await settingsService.SetAsync("TaxRatePercent", "10", "Billing");
        await couponService.CreateAsync(new CreateCouponRequest { Code = "SAVE10", DiscountType = "Flat", DiscountValue = 50 });

        var pricingService = new PricingService(settingsService, couponService);
        var pricing = await pricingService.CalculateAsync(dailyRate: 100, startDate: new DateTime(2026, 1, 1), endDate: new DateTime(2026, 1, 4), couponCode: "save10");

        Assert.Equal(300, pricing.SubtotalAmount);
        Assert.Equal(50, pricing.DiscountAmount);
        // Tax should apply to (300 - 50) = 250, not the full subtotal.
        Assert.Equal(25, pricing.TaxAmount);
        Assert.Equal(275, pricing.TotalAmount);
    }

    [Fact]
    public async Task CalculateAsync_Throws_WhenCouponIsInvalid()
    {
        await using var context = CreateContext();
        var settingsService = new SettingsService(context);
        var couponService = new CouponService(context);
        var pricingService = new PricingService(settingsService, couponService);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pricingService.CalculateAsync(100, new DateTime(2026, 1, 1), new DateTime(2026, 1, 2), "DOES-NOT-EXIST"));
    }

    [Fact]
    public async Task CalculateAsync_Throws_WhenEndDateNotAfterStartDate()
    {
        await using var context = CreateContext();
        var settingsService = new SettingsService(context);
        var couponService = new CouponService(context);
        var pricingService = new PricingService(settingsService, couponService);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            pricingService.CalculateAsync(100, new DateTime(2026, 1, 5), new DateTime(2026, 1, 5), null));
    }
}
