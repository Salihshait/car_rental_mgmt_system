using CarRent.Application.DTOs.Reports;
using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Saas;

public class UsageMonitoringService : IUsageMonitoringService
{
    private readonly CarRentDbContext _context;

    public UsageMonitoringService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task RecordMetricAsync(Guid tenantId, RecordUsageMetricRequest request, CancellationToken cancellationToken = default)
    {
        await _context.TenantUsageMetrics.AddAsync(new TenantUsageMetric
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MetricKey = request.MetricKey,
            MetricValue = request.MetricValue
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlatformOverviewDto> GetPlatformOverviewAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var tenants = await _context.Tenants.AsNoTracking().ToListAsync(cancellationToken);
        var subscriptions = await _context.Subscriptions.AsNoTracking().ToListAsync(cancellationToken);
        var activeSubscriptions = subscriptions.Where(s => s.Status == "Active").ToList();

        var planIds = activeSubscriptions.Select(s => s.PlanId).Distinct().ToList();
        var plans = await _context.SubscriptionPlans.AsNoTracking().Where(p => planIds.Contains(p.Id)).ToListAsync(cancellationToken);

        var mrr = activeSubscriptions.Sum(s =>
        {
            var plan = plans.FirstOrDefault(p => p.Id == s.PlanId);
            if (plan is null) return 0m;
            return s.BillingCycle == "Annual" ? plan.AnnualPrice / 12 : plan.MonthlyPrice;
        });

        var kpis = new List<ReportKpiDto>
        {
            new("Total Tenants", tenants.Count, "number", null),
            new("Trial Tenants", tenants.Count(t => t.Status == "Trial"), "number", null),
            new("Active Subscriptions", activeSubscriptions.Count, "number", null),
            new("MRR", mrr, "currency", null)
        };

        var tenantsInRange = tenants.Where(t => t.CreatedAt >= effectiveFrom && t.CreatedAt <= effectiveTo).ToList();
        var tenantGrowthTrend = tenantsInRange
            .GroupBy(t => new DateTime(t.CreatedAt.Year, t.CreatedAt.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Count()))
            .ToList();

        var subscriptionsInRange = subscriptions.Where(s => s.CreatedAt >= effectiveFrom && s.CreatedAt <= effectiveTo).ToList();
        var mrrTrend = subscriptionsInRange
            .GroupBy(s => new DateTime(s.CreatedAt.Year, s.CreatedAt.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(s =>
            {
                var plan = plans.FirstOrDefault(p => p.Id == s.PlanId);
                if (plan is null) return 0m;
                return s.BillingCycle == "Annual" ? plan.AnnualPrice / 12 : plan.MonthlyPrice;
            })))
            .ToList();

        return new PlatformOverviewDto(kpis, tenantGrowthTrend, mrrTrend);
    }

    public async Task<TenantUsageDto> GetTenantUsageAsync(Guid tenantId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var metrics = await _context.TenantUsageMetrics.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.RecordedAt >= effectiveFrom && m.RecordedAt <= effectiveTo)
            .ToListAsync(cancellationToken);

        var kpis = metrics
            .GroupBy(m => m.MetricKey)
            .Select(g => new ReportKpiDto(g.Key, g.OrderByDescending(m => m.RecordedAt).First().MetricValue, "number", null))
            .ToList();

        var trend = metrics
            .GroupBy(m => new DateTime(m.RecordedAt.Year, m.RecordedAt.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(m => m.MetricValue)))
            .ToList();

        return new TenantUsageDto(kpis, trend);
    }
}
