using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class RevenueReportService : IRevenueReportService
{
    private static readonly string[] ExcludedStatuses = { "Cancelled", "Rejected" };

    private readonly CarRentDbContext _context;

    public RevenueReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query = _context.Bookings.AsNoTracking()
            .Where(b => b.StartDate >= effectiveFrom && b.StartDate <= effectiveTo && !ExcludedStatuses.Contains(b.Status));
        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId);
        }
        var bookings = await query.ToListAsync(cancellationToken);

        var vehicleIds = bookings.Select(b => b.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);
        var branchIds = bookings.Where(b => b.BranchId.HasValue).Select(b => b.BranchId!.Value).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(br => branchIds.Contains(br.Id)).ToListAsync(cancellationToken);
        var modelIds = vehicles.Where(v => v.ModelId.HasValue).Select(v => v.ModelId!.Value).Distinct().ToList();
        var models = await _context.Models.AsNoTracking().Where(m => modelIds.Contains(m.Id)).ToListAsync(cancellationToken);
        var categoryIds = models.Select(m => m.CategoryId).Distinct().ToList();
        var categories = await _context.VehicleCategories.AsNoTracking().Where(c => categoryIds.Contains(c.Id)).ToListAsync(cancellationToken);

        var totalRevenue = bookings.Sum(b => b.TotalAmount);
        var bookingCount = bookings.Count;
        var avgBookingValue = bookingCount == 0 ? 0 : Math.Round(totalRevenue / bookingCount, 2);

        var periodLength = effectiveTo - effectiveFrom;
        var previousFrom = effectiveFrom - periodLength;
        var previousQuery = _context.Bookings.AsNoTracking()
            .Where(b => b.StartDate >= previousFrom && b.StartDate < effectiveFrom && !ExcludedStatuses.Contains(b.Status));
        if (branchId.HasValue)
        {
            previousQuery = previousQuery.Where(b => b.BranchId == branchId);
        }
        var previousRevenue = await previousQuery.SumAsync(b => (decimal?)b.TotalAmount, cancellationToken) ?? 0;
        decimal? growthPercent = previousRevenue == 0 ? null : Math.Round((totalRevenue - previousRevenue) / previousRevenue * 100, 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Revenue", totalRevenue, "currency", growthPercent),
            new("Bookings", bookingCount, "number", null),
            new("Avg Booking Value", avgBookingValue, "currency", null)
        };

        var trend = bookings
            .GroupBy(b => new DateTime(b.StartDate.Year, b.StartDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(b => b.TotalAmount)))
            .ToList();

        var byBranch = bookings
            .GroupBy(b => b.BranchId)
            .Select(g => new ChartPointDto(branches.FirstOrDefault(br => br.Id == g.Key)?.Name ?? "Unassigned", g.Sum(b => b.TotalAmount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        var byCategory = bookings
            .Select(b => new { Booking = b, Vehicle = vehicles.FirstOrDefault(v => v.Id == b.VehicleId) })
            .Select(x => new { x.Booking, Model = x.Vehicle?.ModelId is null ? null : models.FirstOrDefault(m => m.Id == x.Vehicle.ModelId) })
            .GroupBy(x => x.Model?.CategoryId)
            .Select(g => new ChartPointDto(categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Uncategorized", g.Sum(x => x.Booking.TotalAmount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = bookings
            .OrderByDescending(b => b.StartDate)
            .Select(b => new RevenueDetailRowDto(
                b.Id,
                b.StartDate,
                branches.FirstOrDefault(br => br.Id == b.BranchId)?.Name,
                vehicles.FirstOrDefault(v => v.Id == b.VehicleId)?.RegistrationNumber,
                b.Status,
                b.TotalAmount))
            .ToList();

        return new RevenueDashboardDto(kpis, trend, byBranch, byCategory, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, branchId, cancellationToken);

        var model = new ReportExportModel(
            "Revenue Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Revenue Trend", new[] { "Period", "Revenue" }, dashboard.Trend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("By Branch", new[] { "Branch", "Revenue" }, dashboard.ByBranch.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("By Vehicle Category", new[] { "Category", "Revenue" }, dashboard.ByVehicleCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Bookings", new[] { "Booking Id", "Date", "Branch", "Vehicle", "Status", "Amount" },
                    dashboard.DetailRows.Select(r => new[] { r.BookingId.ToString(), r.Date.ToString("d"), r.BranchName ?? "-", r.VehicleRegistrationNumber ?? "-", r.Status, r.Amount.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
