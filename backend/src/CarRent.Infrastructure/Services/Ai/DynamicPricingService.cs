using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class DynamicPricingService : IDynamicPricingService
{
    private static readonly string[] ExcludedBookingStatuses = { "Cancelled", "Rejected" };

    private readonly CarRentDbContext _context;

    public DynamicPricingService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<DynamicPricingResultDto> GetSuggestedPriceAsync(Guid vehicleId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var basePrice = vehicle.DailyRate;
        var factors = new List<PricingFactorDto>();
        var multiplier = 1m;

        var categoryVehicleIds = await GetCategoryVehicleIdsAsync(vehicle, cancellationToken);
        var overlappingBookings = await _context.Bookings.AsNoTracking()
            .CountAsync(b => categoryVehicleIds.Contains(b.VehicleId) && b.StartDate < endDate && b.EndDate > startDate && !ExcludedBookingStatuses.Contains(b.Status), cancellationToken);
        var occupancyRatio = categoryVehicleIds.Count == 0 ? 0 : Math.Min(1m, (decimal)overlappingBookings / categoryVehicleIds.Count);
        var occupancyMultiplier = 1 + occupancyRatio * 0.5m;
        factors.Add(new PricingFactorDto("Category Occupancy", Math.Round(occupancyMultiplier, 2),
            $"{overlappingBookings} of {categoryVehicleIds.Count} vehicles in this category are booked over the requested dates."));
        multiplier *= occupancyMultiplier;

        var isWeekendHeavy = startDate.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday;
        var weekendMultiplier = isWeekendHeavy ? 1.15m : 1.0m;
        factors.Add(new PricingFactorDto("Weekend Demand", weekendMultiplier,
            isWeekendHeavy ? "Pickup falls on a high-demand weekend day." : "Pickup falls on a weekday."));
        multiplier *= weekendMultiplier;

        var leadDays = (startDate - DateTime.UtcNow).TotalDays;
        var leadTimeMultiplier = leadDays switch
        {
            < 2 => 1.2m,
            < 7 => 1.05m,
            _ => 0.95m
        };
        factors.Add(new PricingFactorDto("Booking Lead Time", leadTimeMultiplier,
            leadDays < 2 ? "Last-minute booking." : leadDays < 7 ? "Booked within a week of pickup." : "Booked well in advance."));
        multiplier *= leadTimeMultiplier;

        multiplier = Math.Clamp(multiplier, 0.8m, 1.5m);
        var suggestedPrice = Math.Round(basePrice * multiplier, 2);

        return new DynamicPricingResultDto(basePrice, suggestedPrice, factors);
    }

    private async Task<List<Guid>> GetCategoryVehicleIdsAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        if (!vehicle.ModelId.HasValue)
        {
            return new List<Guid> { vehicle.Id };
        }

        var model = await _context.Models.AsNoTracking().FirstOrDefaultAsync(m => m.Id == vehicle.ModelId, cancellationToken);
        if (model is null)
        {
            return new List<Guid> { vehicle.Id };
        }

        var modelIdsInCategory = await _context.Models.AsNoTracking().Where(m => m.CategoryId == model.CategoryId).Select(m => m.Id).ToListAsync(cancellationToken);
        return await _context.Vehicles.AsNoTracking()
            .Where(v => v.ModelId != null && modelIdsInCategory.Contains(v.ModelId.Value))
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);
    }
}
