using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class FleetReportService : IFleetReportService
{
    private static readonly string[] RentedStatuses = { "On Rent", "Booked", "Reserved" };
    private static readonly string[] MaintenanceStatuses = { "Maintenance", "Accident" };
    private static readonly string[] ExcludedBookingStatuses = { "Cancelled", "Rejected" };

    private readonly CarRentDbContext _context;

    public FleetReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<FleetDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var vehicleQuery = _context.Vehicles.AsNoTracking().AsQueryable();
        if (branchId.HasValue)
        {
            vehicleQuery = vehicleQuery.Where(v => v.BranchId == branchId);
        }
        var vehicles = await vehicleQuery.ToListAsync(cancellationToken);

        var branchIds = vehicles.Select(v => v.BranchId).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(b => branchIds.Contains(b.Id)).ToListAsync(cancellationToken);

        var modelIds = vehicles.Where(v => v.ModelId.HasValue).Select(v => v.ModelId!.Value).Distinct().ToList();
        var models = await _context.Models.AsNoTracking().Where(m => modelIds.Contains(m.Id)).ToListAsync(cancellationToken);
        var categoryIds = models.Select(m => m.CategoryId).Distinct().ToList();
        var categories = await _context.VehicleCategories.AsNoTracking().Where(c => categoryIds.Contains(c.Id)).ToListAsync(cancellationToken);

        var vehicleIds = vehicles.Select(v => v.Id).ToList();
        var bookings = await _context.Bookings.AsNoTracking()
            .Where(b => vehicleIds.Contains(b.VehicleId) && b.StartDate <= effectiveTo && b.EndDate >= effectiveFrom && !ExcludedBookingStatuses.Contains(b.Status))
            .ToListAsync(cancellationToken);

        var totalVehicles = vehicles.Count;
        var available = vehicles.Count(v => v.Status == "Available");
        var rented = vehicles.Count(v => RentedStatuses.Contains(v.Status));
        var inMaintenance = vehicles.Count(v => MaintenanceStatuses.Contains(v.Status));
        var utilizationRate = totalVehicles == 0 ? 0 : Math.Round(rented / (decimal)totalVehicles * 100, 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Vehicles", totalVehicles, "number", null),
            new("Available", available, "number", null),
            new("Rented", rented, "number", null),
            new("In Maintenance", inMaintenance, "number", null),
            new("Utilization Rate", utilizationRate, "percent", null)
        };

        var statusBreakdown = vehicles
            .GroupBy(v => v.Status)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .ToList();

        var utilizationTrend = new List<ChartPointDto>();
        var monthCursor = new DateTime(effectiveFrom.Year, effectiveFrom.Month, 1);
        var lastMonth = new DateTime(effectiveTo.Year, effectiveTo.Month, 1);
        while (monthCursor <= lastMonth)
        {
            var monthStart = monthCursor;
            var monthEnd = monthCursor.AddMonths(1).AddDays(-1);
            var activeVehicles = bookings.Where(b => b.StartDate <= monthEnd && b.EndDate >= monthStart).Select(b => b.VehicleId).Distinct().Count();
            var rate = totalVehicles == 0 ? 0 : Math.Round(activeVehicles / (decimal)totalVehicles * 100, 1);
            utilizationTrend.Add(new ChartPointDto(monthCursor.ToString("MMM yyyy"), rate));
            monthCursor = monthCursor.AddMonths(1);
        }

        var revenueByVehicle = bookings.GroupBy(b => b.VehicleId).ToDictionary(g => g.Key, g => g.Sum(b => b.TotalAmount));
        var bookingCountByVehicle = bookings.GroupBy(b => b.VehicleId).ToDictionary(g => g.Key, g => g.Count());

        var revenueByCategory = vehicles
            .Select(v => new { Vehicle = v, Model = v.ModelId.HasValue ? models.FirstOrDefault(m => m.Id == v.ModelId) : null })
            .GroupBy(x => x.Model?.CategoryId)
            .Select(g => new ChartPointDto(
                categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Uncategorized",
                g.Sum(x => revenueByVehicle.GetValueOrDefault(x.Vehicle.Id))))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = vehicles
            .Select(v => new FleetDetailRowDto(
                v.Id,
                v.RegistrationNumber,
                branches.FirstOrDefault(b => b.Id == v.BranchId)?.Name,
                v.Status,
                bookingCountByVehicle.GetValueOrDefault(v.Id),
                revenueByVehicle.GetValueOrDefault(v.Id)))
            .OrderByDescending(r => r.RevenueGenerated)
            .ToList();

        return new FleetDashboardDto(kpis, statusBreakdown, utilizationTrend, revenueByCategory, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, branchId, cancellationToken);

        var model = new ReportExportModel(
            "Fleet Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Status Breakdown", new[] { "Status", "Count" }, dashboard.StatusBreakdown.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("Utilization Trend", new[] { "Period", "Utilization %" }, dashboard.UtilizationTrend.Select(p => new[] { p.Key, p.Value.ToString("N1") }).ToList()),
                new("Revenue By Category", new[] { "Category", "Revenue" }, dashboard.RevenueByCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Vehicles", new[] { "Vehicle Id", "Registration", "Branch", "Status", "Bookings", "Revenue" },
                    dashboard.DetailRows.Select(r => new[] { r.VehicleId.ToString(), r.RegistrationNumber, r.BranchName ?? "-", r.Status, r.BookingCount.ToString(), r.RevenueGenerated.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
