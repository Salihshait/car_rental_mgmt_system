using CarRent.Application.DTOs.Ai;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class RevenuePredictionService : IRevenuePredictionService
{
    private readonly CarRentDbContext _context;

    public RevenuePredictionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<ForecastDto> GetForecastAsync(int monthsAhead, CancellationToken cancellationToken = default)
    {
        var historyStart = DateTime.UtcNow.AddMonths(-6).Date;
        var payments = await _context.Payments.AsNoTracking()
            .Where(p => p.Status == "Verified" && p.PaidAt >= historyStart)
            .ToListAsync(cancellationToken);

        var monthlyRevenue = payments
            .GroupBy(p => new DateTime(p.PaidAt!.Value.Year, p.PaidAt.Value.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => (Month: g.Key, Total: g.Sum(p => p.Amount)))
            .ToList();

        var trend = monthlyRevenue.Select(m => new ChartPointDto(m.Month.ToString("MMM yyyy"), m.Total)).ToList();

        var avgDelta = 0m;
        if (monthlyRevenue.Count >= 2)
        {
            var deltas = new List<decimal>();
            for (var i = 1; i < monthlyRevenue.Count; i++)
            {
                deltas.Add(monthlyRevenue[i].Total - monthlyRevenue[i - 1].Total);
            }
            avgDelta = deltas.Average();
        }

        var lastTotal = monthlyRevenue.Count > 0 ? monthlyRevenue[^1].Total : 0;
        var cursor = monthlyRevenue.Count > 0 ? monthlyRevenue[^1].Month : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var forecastPoints = new List<ChartPointDto>();
        var runningValue = lastTotal;
        for (var i = 0; i < monthsAhead; i++)
        {
            cursor = cursor.AddMonths(1);
            runningValue = Math.Max(0, runningValue + avgDelta);
            forecastPoints.Add(new ChartPointDto($"{cursor:MMM yyyy} (forecast)", Math.Round(runningValue, 2)));
        }

        trend.AddRange(forecastPoints);

        var totalHistorical = payments.Sum(p => p.Amount);
        var kpis = new List<ReportKpiDto>
        {
            new("Historical Revenue (6mo)", totalHistorical, "currency", null),
            new("Avg Monthly Revenue", monthlyRevenue.Count == 0 ? 0 : Math.Round(totalHistorical / monthlyRevenue.Count, 2), "currency", null),
            new("Projected Next Month", forecastPoints.Count > 0 ? forecastPoints[0].Value : 0, "currency", null)
        };

        return new ForecastDto(kpis, trend);
    }
}
