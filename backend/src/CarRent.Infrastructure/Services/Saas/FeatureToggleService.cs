using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class FeatureToggleService : IFeatureToggleService
{
    private readonly CarRentDbContext _context;

    public FeatureToggleService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TenantFeatureOverrideDto>> GetTenantOverridesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var overrides = await _context.TenantFeatureOverrides.AsNoTracking().Where(o => o.TenantId == tenantId).ToListAsync(cancellationToken);
        return overrides.Select(o => new TenantFeatureOverrideDto(o.Id, o.FeatureKey, o.IsEnabled));
    }

    public async Task<TenantFeatureOverrideDto> UpsertTenantOverrideAsync(Guid tenantId, UpsertTenantFeatureOverrideRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TenantFeatureOverrides
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.FeatureKey == request.FeatureKey, cancellationToken);

        if (existing is null)
        {
            existing = new TenantFeatureOverride { Id = Guid.NewGuid(), TenantId = tenantId, FeatureKey = request.FeatureKey };
            await _context.TenantFeatureOverrides.AddAsync(existing, cancellationToken);
        }

        existing.IsEnabled = request.IsEnabled;
        await _context.SaveChangesAsync(cancellationToken);

        return new TenantFeatureOverrideDto(existing.Id, existing.FeatureKey, existing.IsEnabled);
    }

    public async Task<FeatureResolutionDto> ResolveAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default)
    {
        var overrideEntry = await _context.TenantFeatureOverrides.AsNoTracking()
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.FeatureKey == featureKey, cancellationToken);
        if (overrideEntry is not null)
        {
            return new FeatureResolutionDto(featureKey, overrideEntry.IsEnabled, "Override");
        }

        var subscription = await _context.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.Status == "Active")
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is not null)
        {
            var planFeature = await _context.PlanFeatures.AsNoTracking()
                .FirstOrDefaultAsync(f => f.PlanId == subscription.PlanId && f.FeatureKey == featureKey, cancellationToken);
            if (planFeature is not null)
            {
                return new FeatureResolutionDto(featureKey, planFeature.IsEnabled, "Plan");
            }
        }

        return new FeatureResolutionDto(featureKey, false, "Default");
    }
}
