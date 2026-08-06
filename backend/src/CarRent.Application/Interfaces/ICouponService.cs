using CarRent.Application.DTOs.Coupons;

namespace CarRent.Application.Interfaces;

public interface ICouponService
{
    Task<IEnumerable<CouponDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CouponDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CouponDto> CreateAsync(CreateCouponRequest request, CancellationToken cancellationToken = default);
    Task<CouponDto> UpdateAsync(Guid id, UpdateCouponRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ValidateCouponResultDto> ValidateAsync(string code, decimal subtotalAmount, CancellationToken cancellationToken = default);

    /// <summary>Records usage against a coupon at booking-creation time; caller has already validated via ValidateAsync.</summary>
    Task RedeemAsync(Guid couponId, Guid bookingId, Guid customerId, decimal discountApplied, CancellationToken cancellationToken = default);
}
