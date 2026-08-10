using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class PlanLimitService : IPlanLimitService
{
    private readonly CarRentDbContext _context;

    public PlanLimitService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EffectiveLimitDto>> GetEffectiveLimitsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.Status == "Active")
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return Enumerable.Empty<EffectiveLimitDto>();
        }

        var plan = await _context.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == subscription.PlanId, cancellationToken);
        var limits = await _context.PlanLimits.AsNoTracking().Where(l => l.PlanId == subscription.PlanId).ToListAsync(cancellationToken);

        return limits.Select(l => new EffectiveLimitDto(l.LimitKey, l.LimitValue, plan?.Name));
    }

    public async Task<bool> CheckLimitAsync(Guid tenantId, string limitKey, int currentUsage, CancellationToken cancellationToken = default)
    {
        var limits = await GetEffectiveLimitsAsync(tenantId, cancellationToken);
        var limit = limits.FirstOrDefault(l => l.LimitKey == limitKey);
        if (limit is null)
        {
            return true;
        }

        return limit.LimitValue < 0 || currentUsage <= limit.LimitValue;
    }
}
