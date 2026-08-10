using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class RecommendationEngineService : IRecommendationEngineService
{
    private const int MaxResults = 5;

    private readonly CarRentDbContext _context;

    public RecommendationEngineService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleRecommendationDto>> GetRecommendationsAsync(Guid customerUserId, CancellationToken cancellationToken = default)
    {
        var allVehicles = await _context.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
        var modelIds = allVehicles.Where(v => v.ModelId.HasValue).Select(v => v.ModelId!.Value).Distinct().ToList();
        var models = await _context.Models.AsNoTracking().Where(m => modelIds.Contains(m.Id)).ToListAsync(cancellationToken);
        var brands = await _context.Brands.AsNoTracking().ToListAsync(cancellationToken);

        Guid? CategoryOf(Guid vehicleId)
        {
            var vehicle = allVehicles.FirstOrDefault(v => v.Id == vehicleId);
            return vehicle?.ModelId is null ? null : models.FirstOrDefault(m => m.Id == vehicle.ModelId)?.CategoryId;
        }

        var availableVehicles = allVehicles.Where(v => v.Status == "Available").ToList();
        var favorites = await _context.FavoriteVehicles.AsNoTracking().Where(f => f.CustomerId == customerUserId).ToListAsync(cancellationToken);
        var pastBookings = await _context.Bookings.AsNoTracking().Where(b => b.CustomerId == customerUserId).ToListAsync(cancellationToken);
        var excludeIds = favorites.Select(f => f.VehicleId).Concat(pastBookings.Select(b => b.VehicleId)).ToHashSet();

        var popularity = await _context.Bookings.AsNoTracking()
            .GroupBy(b => b.VehicleId)
            .Select(g => new { VehicleId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var results = new List<VehicleRecommendationDto>();

        var favoriteCategoryIds = favorites.Select(f => CategoryOf(f.VehicleId)).Where(c => c.HasValue).Select(c => c!.Value).ToHashSet();
        if (favoriteCategoryIds.Count > 0)
        {
            results.AddRange(availableVehicles
                .Where(v => !excludeIds.Contains(v.Id) && favoriteCategoryIds.Contains(CategoryOf(v.Id) ?? Guid.Empty))
                .Take(MaxResults)
                .Select(v => ToDto(v, models, brands, "Similar to vehicles in your favorites")));
        }

        if (results.Count < MaxResults)
        {
            var pastCategoryIds = pastBookings.Select(b => CategoryOf(b.VehicleId)).Where(c => c.HasValue).Select(c => c!.Value).ToHashSet();
            if (pastCategoryIds.Count > 0)
            {
                var alreadyRecommended = results.Select(r => r.VehicleId).ToHashSet();
                results.AddRange(availableVehicles
                    .Where(v => !excludeIds.Contains(v.Id) && !alreadyRecommended.Contains(v.Id) && pastCategoryIds.Contains(CategoryOf(v.Id) ?? Guid.Empty))
                    .OrderByDescending(v => popularity.FirstOrDefault(p => p.VehicleId == v.Id)?.Count ?? 0)
                    .Take(MaxResults - results.Count)
                    .Select(v => ToDto(v, models, brands, "Popular in a category you've rented before")));
            }
        }

        if (results.Count < MaxResults)
        {
            var alreadyRecommended = results.Select(r => r.VehicleId).ToHashSet();
            results.AddRange(availableVehicles
                .Where(v => !excludeIds.Contains(v.Id) && !alreadyRecommended.Contains(v.Id))
                .OrderByDescending(v => popularity.FirstOrDefault(p => p.VehicleId == v.Id)?.Count ?? 0)
                .Take(MaxResults - results.Count)
                .Select(v => ToDto(v, models, brands, "Popular with other customers")));
        }

        return results;
    }

    private static VehicleRecommendationDto ToDto(Vehicle vehicle, List<Model> models, List<Brand> brands, string reason)
    {
        var model = vehicle.ModelId.HasValue ? models.FirstOrDefault(m => m.Id == vehicle.ModelId) : null;
        var brand = vehicle.BrandId.HasValue ? brands.FirstOrDefault(b => b.Id == vehicle.BrandId) : null;
        return new VehicleRecommendationDto(vehicle.Id, vehicle.RegistrationNumber, brand?.Name, model?.Name, vehicle.DailyRate, reason);
    }
}
