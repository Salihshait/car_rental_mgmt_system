using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly CarRentDbContext _context;

    public SubscriptionPlanService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync(bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _context.SubscriptionPlans.AsNoTracking().AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var plans = await query.OrderBy(p => p.MonthlyPrice).ToListAsync(cancellationToken);
        var planIds = plans.Select(p => p.Id).ToList();

        var limits = await _context.PlanLimits.AsNoTracking().Where(l => planIds.Contains(l.PlanId)).ToListAsync(cancellationToken);
        var features = await _context.PlanFeatures.AsNoTracking().Where(f => planIds.Contains(f.PlanId)).ToListAsync(cancellationToken);

        return plans.Select(p => MapDto(p, limits.Where(l => l.PlanId == p.Id).ToList(), features.Where(f => f.PlanId == p.Id).ToList()));
    }

    public async Task<SubscriptionPlanDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        var limits = await _context.PlanLimits.AsNoTracking().Where(l => l.PlanId == id).ToListAsync(cancellationToken);
        var features = await _context.PlanFeatures.AsNoTracking().Where(f => f.PlanId == id).ToListAsync(cancellationToken);

        return MapDto(plan, limits, features);
    }

    public async Task<SubscriptionPlanDto> CreateAsync(UpsertPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            Currency = request.Currency,
            IsActive = request.IsActive
        };

        await _context.SubscriptionPlans.AddAsync(plan, cancellationToken);
        await ReplaceLimitsAndFeaturesAsync(plan.Id, request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(plan.Id, cancellationToken);
    }

    public async Task<SubscriptionPlanDto> UpdateAsync(Guid id, UpsertPlanRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.AnnualPrice = request.AnnualPrice;
        plan.Currency = request.Currency;
        plan.IsActive = request.IsActive;

        var existingLimits = await _context.PlanLimits.Where(l => l.PlanId == id).ToListAsync(cancellationToken);
        _context.PlanLimits.RemoveRange(existingLimits);
        var existingFeatures = await _context.PlanFeatures.Where(f => f.PlanId == id).ToListAsync(cancellationToken);
        _context.PlanFeatures.RemoveRange(existingFeatures);

        await ReplaceLimitsAndFeaturesAsync(id, request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task ReplaceLimitsAndFeaturesAsync(Guid planId, UpsertPlanRequest request, CancellationToken cancellationToken)
    {
        foreach (var limit in request.Limits)
        {
            await _context.PlanLimits.AddAsync(new PlanLimit { Id = Guid.NewGuid(), PlanId = planId, LimitKey = limit.LimitKey, LimitValue = limit.LimitValue }, cancellationToken);
        }

        foreach (var feature in request.Features)
        {
            await _context.PlanFeatures.AddAsync(new PlanFeature { Id = Guid.NewGuid(), PlanId = planId, FeatureKey = feature.FeatureKey, IsEnabled = feature.IsEnabled }, cancellationToken);
        }
    }

    private static SubscriptionPlanDto MapDto(SubscriptionPlan plan, List<PlanLimit> limits, List<PlanFeature> features) => new(
        plan.Id, plan.Name, plan.Description, plan.MonthlyPrice, plan.AnnualPrice, plan.Currency, plan.IsActive,
        limits.Select(l => new PlanLimitDto(l.Id, l.LimitKey, l.LimitValue)).ToList(),
        features.Select(f => new PlanFeatureDto(f.Id, f.FeatureKey, f.IsEnabled)).ToList(),
        plan.CreatedAt);
}
