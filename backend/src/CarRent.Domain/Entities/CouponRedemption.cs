namespace CarRent.Domain.Entities;

public class CouponRedemption
{
    public Guid Id { get; set; }
    public Guid CouponId { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal DiscountApplied { get; set; }
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}
