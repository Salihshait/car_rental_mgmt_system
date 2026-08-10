using CarRent.Application.DTOs.Ai;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class DemandPredictionService : IDemandPredictionService
{
    private readonly CarRentDbContext _context;

    public DemandPredictionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<ForecastDto> GetForecastAsync(Guid? branchId, Guid? categoryId, int monthsAhead, CancellationToken cancellationToken = default)
    {
        var historyStart = DateTime.UtcNow.AddMonths(-6).Date;
        var query = _context.Bookings.AsNoTracking().Where(b => b.StartDate >= historyStart);
        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId);
        }

        if (categoryId.HasValue)
        {
            var modelIds = await _context.Models.AsNoTracking().Where(m => m.CategoryId == categoryId).Select(m => m.Id).ToListAsync(cancellationToken);
            var categoryVehicleIds = await _context.Vehicles.AsNoTracking()
                .Where(v => v.ModelId != null && modelIds.Contains(v.ModelId.Value))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);
            query = query.Where(b => categoryVehicleIds.Contains(b.VehicleId));
        }

        var bookings = await query.ToListAsync(cancellationToken);

        var monthlyCounts = bookings
            .GroupBy(b => new DateTime(b.StartDate.Year, b.StartDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => (Month: g.Key, Count: g.Count()))
            .ToList();

        var trend = monthlyCounts.Select(m => new ChartPointDto(m.Month.ToString("MMM yyyy"), m.Count)).ToList();

        var avgDelta = 0m;
        if (monthlyCounts.Count >= 2)
        {
            var deltas = new List<int>();
            for (var i = 1; i < monthlyCounts.Count; i++)
            {
                deltas.Add(monthlyCounts[i].Count - monthlyCounts[i - 1].Count);
            }
            avgDelta = (decimal)deltas.Average();
        }

        var lastCount = monthlyCounts.Count > 0 ? monthlyCounts[^1].Count : 0;
        var cursor = monthlyCounts.Count > 0 ? monthlyCounts[^1].Month : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var forecastPoints = new List<ChartPointDto>();
        var runningValue = (decimal)lastCount;
        for (var i = 0; i < monthsAhead; i++)
        {
            cursor = cursor.AddMonths(1);
            runningValue = Math.Max(0, runningValue + avgDelta);
            forecastPoints.Add(new ChartPointDto($"{cursor:MMM yyyy} (forecast)", Math.Round(runningValue)));
        }

        trend.AddRange(forecastPoints);

        var kpis = new List<ReportKpiDto>
        {
            new("Historical Bookings (6mo)", bookings.Count, "number", null),
            new("Avg Monthly Bookings", monthlyCounts.Count == 0 ? 0 : Math.Round((decimal)bookings.Count / monthlyCounts.Count, 1), "number", null),
            new("Projected Next Month", forecastPoints.Count > 0 ? forecastPoints[0].Value : 0, "number", null)
        };

        return new ForecastDto(kpis, trend);
    }
}
