using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class BookingsReportService : IBookingsReportService
{
    private readonly CarRentDbContext _context;

    public BookingsReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<BookingsDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query = _context.Bookings.AsNoTracking()
            .Where(b => b.StartDate >= effectiveFrom && b.StartDate <= effectiveTo);
        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId);
        }
        var bookings = await query.ToListAsync(cancellationToken);

        var vehicleIds = bookings.Select(b => b.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);
        var branchIds = bookings.Where(b => b.BranchId.HasValue).Select(b => b.BranchId!.Value).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(br => branchIds.Contains(br.Id)).ToListAsync(cancellationToken);

        var total = bookings.Count;
        var completed = bookings.Count(b => b.Status == "Completed");
        var cancelled = bookings.Count(b => b.Status is "Cancelled" or "Rejected");
        var cancellationRate = total == 0 ? 0 : Math.Round(cancelled / (decimal)total * 100, 1);
        var avgDurationDays = total == 0 ? 0 : Math.Round((decimal)bookings.Average(b => (b.EndDate - b.StartDate).TotalDays), 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Bookings", total, "number", null),
            new("Completed", completed, "number", null),
            new("Cancellation Rate", cancellationRate, "percent", null),
            new("Avg Duration (days)", avgDurationDays, "number", null)
        };

        var trend = bookings
            .GroupBy(b => new DateTime(b.StartDate.Year, b.StartDate.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Count()))
            .ToList();

        var byStatus = bookings
            .GroupBy(b => b.Status)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .ToList();

        var byBranch = bookings
            .GroupBy(b => b.BranchId)
            .Select(g => new ChartPointDto(branches.FirstOrDefault(br => br.Id == g.Key)?.Name ?? "Unassigned", g.Count()))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = bookings
            .OrderByDescending(b => b.StartDate)
            .Select(b => new BookingDetailRowDto(
                b.Id,
                b.StartDate,
                b.EndDate,
                branches.FirstOrDefault(br => br.Id == b.BranchId)?.Name,
                vehicles.FirstOrDefault(v => v.Id == b.VehicleId)?.RegistrationNumber,
                b.Status,
                b.TotalAmount))
            .ToList();

        return new BookingsDashboardDto(kpis, trend, byStatus, byBranch, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, branchId, cancellationToken);

        var model = new ReportExportModel(
            "Bookings Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Bookings Trend", new[] { "Period", "Bookings" }, dashboard.Trend.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("By Status", new[] { "Status", "Count" }, dashboard.ByStatus.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("By Branch", new[] { "Branch", "Count" }, dashboard.ByBranch.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("Bookings", new[] { "Booking Id", "Start", "End", "Branch", "Vehicle", "Status", "Amount" },
                    dashboard.DetailRows.Select(r => new[] { r.BookingId.ToString(), r.StartDate.ToString("d"), r.EndDate.ToString("d"), r.BranchName ?? "-", r.VehicleRegistrationNumber ?? "-", r.Status, r.TotalAmount.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
