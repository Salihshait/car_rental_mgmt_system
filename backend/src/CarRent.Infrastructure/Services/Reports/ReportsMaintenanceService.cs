using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Reports;

public class ReportsMaintenanceService : IReportsMaintenanceService
{
    private static readonly string[] OpenStatuses = { "Scheduled", "InProgress" };

    private readonly IMaintenanceReportService _maintenanceReportService;
    private readonly CarRentDbContext _context;

    public ReportsMaintenanceService(IMaintenanceReportService maintenanceReportService, CarRentDbContext context)
    {
        _maintenanceReportService = maintenanceReportService;
        _context = context;
    }

    public async Task<MaintenanceDashboardDto> GetDashboardAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var costSummary = await _maintenanceReportService.GetCostSummaryAsync(effectiveFrom, effectiveTo, null, null, null, cancellationToken);
        var vendorPerformance = (await _maintenanceReportService.GetVendorPerformanceAsync(effectiveFrom, effectiveTo, cancellationToken)).ToList();

        var openWorkOrders = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => OpenStatuses.Contains(m.Status) && m.ScheduledOn >= effectiveFrom && m.ScheduledOn <= effectiveTo)
            .ToListAsync(cancellationToken);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Cost", costSummary.TotalCost, "currency", null),
            new("Open Work Orders", openWorkOrders.Count, "number", null),
            new("Avg Cost / Vehicle", costSummary.CostByVehicle.Count == 0 ? 0 : Math.Round(costSummary.TotalCost / costSummary.CostByVehicle.Count, 2), "currency", null),
            new("Vendors", vendorPerformance.Count, "number", null)
        };

        var costByCategory = costSummary.CostByCategory
            .Select(kv => new ChartPointDto(kv.Key, kv.Value))
            .OrderByDescending(p => p.Value)
            .ToList();

        var vendorPerformanceChart = vendorPerformance
            .Select(v => new ChartPointDto(v.VendorName ?? "Unknown", v.TotalSpend))
            .OrderByDescending(p => p.Value)
            .ToList();

        var openWorkOrdersByType = openWorkOrders
            .GroupBy(m => m.MaintenanceType)
            .Select(g => new ChartPointDto(g.Key, g.Count()))
            .OrderByDescending(p => p.Value)
            .ToList();

        var detailRows = costSummary.CostByVehicle
            .Select(v => new MaintenanceDetailRowDto(v.VehicleId, v.RegistrationNumber, v.TotalCost))
            .ToList();

        return new MaintenanceDashboardDto(kpis, costByCategory, vendorPerformanceChart, openWorkOrdersByType, detailRows);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var dashboard = await GetDashboardAsync(from, to, cancellationToken);

        var model = new ReportExportModel(
            "Maintenance Report",
            from,
            to,
            dashboard.Kpis,
            new List<ReportExportSection>
            {
                new("Cost By Category", new[] { "Category", "Cost" }, dashboard.CostByCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Vendor Performance", new[] { "Vendor", "Total Spend" }, dashboard.VendorPerformance.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Open Work Orders By Type", new[] { "Type", "Count" }, dashboard.OpenWorkOrdersByType.Select(p => new[] { p.Key, p.Value.ToString("N0") }).ToList()),
                new("Cost By Vehicle", new[] { "Vehicle Id", "Registration", "Total Cost" },
                    dashboard.DetailRows.Select(r => new[] { r.VehicleId.ToString(), r.RegistrationNumber ?? "-", r.TotalCost.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
