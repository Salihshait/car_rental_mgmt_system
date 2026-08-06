using CarRent.Application.DTOs.Bookings;

namespace CarRent.Application.Interfaces;

public interface IPricingService
{
    Task<PricingBreakdownDto> CalculateAsync(decimal dailyRate, DateTime startDate, DateTime endDate, string? couponCode, CancellationToken cancellationToken = default);
}
