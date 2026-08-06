using CarRent.Application.DTOs.Coupons;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly CarRentDbContext _context;

    public CouponService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CouponDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken cancellationToken = default)
    {
        ValidateDiscount(request.DiscountType, request.DiscountValue);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await _context.Coupons.AnyAsync(c => c.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("A coupon with this code already exists.");
        }

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinBookingAmount = request.MinBookingAmount,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UsageLimit = request.UsageLimit,
            IsActive = true
        };

        await _context.Coupons.AddAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(coupon.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created coupon.");
    }

    public async Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken cancellationToken = default)
    {
        ValidateDiscount(request.DiscountType, request.DiscountValue);

        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Coupon not found.");

        coupon.Description = request.Description;
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinBookingAmount = request.MinBookingAmount;
        coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.UsageLimit = request.UsageLimit;
        coupon.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated coupon.");
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Coupon not found.");

        coupon.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ValidateCouponResultDto> ValidateAsync(string code, decimal subtotalAmount, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);

        if (coupon is null)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = "Coupon code not found." };
        }

        if (!coupon.IsActive)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = "This coupon is no longer active." };
        }

        var now = DateTime.UtcNow;
        if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = "This coupon is not active yet." };
        }

        if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = "This coupon has expired." };
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = "This coupon has reached its usage limit." };
        }

        if (coupon.MinBookingAmount.HasValue && subtotalAmount < coupon.MinBookingAmount.Value)
        {
            return new ValidateCouponResultDto { IsValid = false, Message = $"This coupon requires a minimum booking amount of {coupon.MinBookingAmount.Value:C}." };
        }

        var discount = coupon.DiscountType == "Percentage"
            ? Math.Round(subtotalAmount * coupon.DiscountValue / 100m, 2)
            : coupon.DiscountValue;

        if (coupon.MaxDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
        }

        discount = Math.Min(discount, subtotalAmount);

        return new ValidateCouponResultDto { IsValid = true, DiscountAmount = discount };
    }

    public async Task RedeemAsync(Guid couponId, Guid bookingId, Guid customerId, decimal discountApplied, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken)
            ?? throw new InvalidOperationException("Coupon not found.");

        coupon.UsageCount += 1;

        await _context.CouponRedemptions.AddAsync(new CouponRedemption
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            BookingId = bookingId,
            CustomerId = customerId,
            DiscountApplied = discountApplied
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateDiscount(string discountType, decimal discountValue)
    {
        if (discountType is not ("Percentage" or "Flat"))
        {
            throw new InvalidOperationException("DiscountType must be 'Percentage' or 'Flat'.");
        }

        if (discountValue <= 0)
        {
            throw new InvalidOperationException("DiscountValue must be greater than zero.");
        }

        if (discountType == "Percentage" && discountValue > 100)
        {
            throw new InvalidOperationException("Percentage discount cannot exceed 100.");
        }
    }

    private IQueryable<CouponDto> Query()
    {
        return _context.Coupons
            .AsNoTracking()
            .Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinBookingAmount = c.MinBookingAmount,
                MaxDiscountAmount = c.MaxDiscountAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                UsageCount = c.UsageCount,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            });
    }
}
