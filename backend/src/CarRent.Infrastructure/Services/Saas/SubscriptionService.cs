using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class SubscriptionService : ISubscriptionService
{
    private readonly CarRentDbContext _context;

    public SubscriptionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto> CreateAsync(Guid tenantId, CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        var periodStart = DateTime.UtcNow;
        var periodEnd = request.BillingCycle == "Annual" ? periodStart.AddYears(1) : periodStart.AddMonths(1);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = request.PlanId,
            Status = "Active",
            BillingCycle = request.BillingCycle,
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd
        };

        await _context.Subscriptions.AddAsync(subscription, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(subscription, plan.Name);
    }

    public async Task<IEnumerable<SubscriptionDto>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _context.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var planIds = subscriptions.Select(s => s.PlanId).Distinct().ToList();
        var plans = await _context.SubscriptionPlans.AsNoTracking().Where(p => planIds.Contains(p.Id)).ToListAsync(cancellationToken);

        return subscriptions.Select(s => MapDto(s, plans.FirstOrDefault(p => p.Id == s.PlanId)?.Name));
    }

    public async Task<SubscriptionDto> CancelAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        subscription.Status = "Cancelled";
        subscription.CancelAtPeriodEnd = true;
        await _context.SaveChangesAsync(cancellationToken);

        var plan = await _context.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == subscription.PlanId, cancellationToken);
        return MapDto(subscription, plan?.Name);
    }

    private static SubscriptionDto MapDto(Subscription subscription, string? planName) => new(
        subscription.Id, subscription.TenantId, subscription.PlanId, planName, subscription.Status, subscription.BillingCycle,
        subscription.CurrentPeriodStart, subscription.CurrentPeriodEnd, subscription.CancelAtPeriodEnd, subscription.CreatedAt);
}
