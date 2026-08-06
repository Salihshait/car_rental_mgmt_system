using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class MaintenanceReportService : IMaintenanceReportService
{
    private readonly CarRentDbContext _context;

    public MaintenanceReportService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MaintenanceCalendarEntryDto>> GetCalendarAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var entries = new List<MaintenanceCalendarEntryDto>();

        var vehicleIds = new HashSet<Guid>();

        var maintenanceRecords = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "Scheduled" && m.ScheduledOn >= from && m.ScheduledOn <= to)
            .ToListAsync(cancellationToken);
        foreach (var m in maintenanceRecords)
        {
            vehicleIds.Add(m.VehicleId);
            entries.Add(new MaintenanceCalendarEntryDto { Type = "Maintenance", SourceId = m.Id, VehicleId = m.VehicleId, Title = m.ServiceType, DueDate = m.ScheduledOn, Status = m.Status });
        }

        var amcContracts = await _context.AmcContracts.AsNoTracking()
            .Where(c => c.EndDate >= from && c.EndDate <= to)
            .ToListAsync(cancellationToken);
        foreach (var c in amcContracts)
        {
            vehicleIds.Add(c.VehicleId);
            entries.Add(new MaintenanceCalendarEntryDto { Type = "AMC", SourceId = c.Id, VehicleId = c.VehicleId, Title = $"AMC {c.ContractNumber}", DueDate = c.EndDate, Status = MaintenanceStatusHelper.ComputeExpiryStatus(c.Status, c.EndDate) });
        }

        var warranties = await _context.VehicleWarranties.AsNoTracking()
            .Where(w => w.EndDate >= from && w.EndDate <= to)
            .ToListAsync(cancellationToken);
        foreach (var w in warranties)
        {
            vehicleIds.Add(w.VehicleId);
            entries.Add(new MaintenanceCalendarEntryDto { Type = "Warranty", SourceId = w.Id, VehicleId = w.VehicleId, Title = w.WarrantyType, DueDate = w.EndDate, Status = MaintenanceStatusHelper.ComputeExpiryStatus(w.Status, w.EndDate) });
        }

        var inspections = await _context.VehicleInspections.AsNoTracking()
            .Where(i => i.NextDueDate.HasValue && i.NextDueDate >= from && i.NextDueDate <= to)
            .ToListAsync(cancellationToken);
        foreach (var i in inspections)
        {
            vehicleIds.Add(i.VehicleId);
            entries.Add(new MaintenanceCalendarEntryDto { Type = "Inspection", SourceId = i.Id, VehicleId = i.VehicleId, Title = i.InspectionType, DueDate = i.NextDueDate!.Value, Status = i.Result });
        }

        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);
        foreach (var entry in entries)
        {
            entry.VehicleRegistrationNumber = vehicles.FirstOrDefault(v => v.Id == entry.VehicleId)?.RegistrationNumber;
        }

        return entries.OrderBy(e => e.DueDate).ToList();
    }

    public async Task<MaintenanceCostSummaryDto> GetCostSummaryAsync(DateTime? from, DateTime? to, Guid? vehicleId, Guid? workshopId, Guid? vendorId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveTo = to ?? DateTime.UtcNow;

        var maintenanceQuery = _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "Completed" && m.ScheduledOn >= effectiveFrom && m.ScheduledOn <= effectiveTo);
        if (vehicleId.HasValue) maintenanceQuery = maintenanceQuery.Where(m => m.VehicleId == vehicleId);
        if (workshopId.HasValue) maintenanceQuery = maintenanceQuery.Where(m => m.WorkshopId == workshopId);
        var maintenanceRecords = await maintenanceQuery.ToListAsync(cancellationToken);
        var maintenanceCost = maintenanceRecords.Sum(m => m.Cost ?? 0);

        var maintenanceIds = maintenanceRecords.Select(m => m.Id).ToList();
        var partsUsages = await _context.MaintenancePartUsages.AsNoTracking()
            .Where(u => maintenanceIds.Contains(u.MaintenanceId))
            .ToListAsync(cancellationToken);
        var partsCost = partsUsages.Sum(u => u.TotalAmount);

        var expensesQuery = _context.MaintenanceExpenses.AsNoTracking()
            .Where(e => e.ExpenseDate >= effectiveFrom && e.ExpenseDate <= effectiveTo);
        if (vehicleId.HasValue) expensesQuery = expensesQuery.Where(e => e.VehicleId == vehicleId);
        if (vendorId.HasValue) expensesQuery = expensesQuery.Where(e => e.VendorId == vendorId);
        var expenses = await expensesQuery.ToListAsync(cancellationToken);
        var expensesCost = expenses.Sum(e => e.Amount);

        var costByCategory = new Dictionary<string, decimal>
        {
            ["Maintenance"] = maintenanceCost,
            ["Parts"] = partsCost
        };
        foreach (var group in expenses.GroupBy(e => e.Category))
        {
            costByCategory[group.Key] = costByCategory.GetValueOrDefault(group.Key) + group.Sum(e => e.Amount);
        }

        var vehicleTotals = new Dictionary<Guid, decimal>();
        foreach (var m in maintenanceRecords)
        {
            vehicleTotals[m.VehicleId] = vehicleTotals.GetValueOrDefault(m.VehicleId) + (m.Cost ?? 0);
        }
        foreach (var usage in partsUsages)
        {
            var owningMaintenance = maintenanceRecords.FirstOrDefault(m => m.Id == usage.MaintenanceId);
            if (owningMaintenance is not null)
            {
                vehicleTotals[owningMaintenance.VehicleId] = vehicleTotals.GetValueOrDefault(owningMaintenance.VehicleId) + usage.TotalAmount;
            }
        }
        foreach (var expense in expenses.Where(e => e.VehicleId.HasValue))
        {
            vehicleTotals[expense.VehicleId!.Value] = vehicleTotals.GetValueOrDefault(expense.VehicleId.Value) + expense.Amount;
        }

        var vehicleIds = vehicleTotals.Keys.ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var costByVehicle = vehicleTotals
            .Select(kv => new VehicleCostDto { VehicleId = kv.Key, RegistrationNumber = vehicles.FirstOrDefault(v => v.Id == kv.Key)?.RegistrationNumber, TotalCost = kv.Value })
            .OrderByDescending(v => v.TotalCost)
            .ToList();

        return new MaintenanceCostSummaryDto
        {
            TotalCost = maintenanceCost + partsCost + expensesCost,
            MaintenanceCost = maintenanceCost,
            PartsCost = partsCost,
            ExpensesCost = expensesCost,
            CostByCategory = costByCategory,
            CostByVehicle = costByVehicle
        };
    }

    public async Task<IEnumerable<VendorPerformanceDto>> GetVendorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveTo = to ?? DateTime.UtcNow;

        var records = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "Completed" && m.WorkshopId != null && m.ScheduledOn >= effectiveFrom && m.ScheduledOn <= effectiveTo)
            .ToListAsync(cancellationToken);

        var workshopIds = records.Select(r => r.WorkshopId!.Value).Distinct().ToList();
        var workshops = await _context.Workshops.AsNoTracking().Where(w => workshopIds.Contains(w.Id)).ToListAsync(cancellationToken);

        var vendorIds = workshops.Where(w => w.VendorId.HasValue).Select(w => w.VendorId!.Value).Distinct().ToList();
        var vendors = await _context.Vendors.AsNoTracking().Where(v => vendorIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return records
            .Select(r => new { Record = r, Workshop = workshops.FirstOrDefault(w => w.Id == r.WorkshopId) })
            .Where(x => x.Workshop?.VendorId is not null)
            .GroupBy(x => x.Workshop!.VendorId!.Value)
            .Select(g => new VendorPerformanceDto
            {
                VendorId = g.Key,
                VendorName = vendors.FirstOrDefault(v => v.Id == g.Key)?.Name,
                JobCount = g.Count(),
                TotalSpend = g.Sum(x => x.Record.Cost ?? 0),
                AverageCost = g.Count() == 0 ? 0 : Math.Round(g.Sum(x => x.Record.Cost ?? 0) / g.Count(), 2)
            })
            .OrderByDescending(v => v.TotalSpend)
            .ToList();
    }

    public async Task<SparePartsConsumptionReportDto> GetSparePartsConsumptionAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-1);
        var effectiveTo = to ?? DateTime.UtcNow;

        var usages = await _context.MaintenancePartUsages.AsNoTracking()
            .Where(u => u.CreatedAt >= effectiveFrom && u.CreatedAt <= effectiveTo)
            .ToListAsync(cancellationToken);

        var allParts = await _context.SpareParts.AsNoTracking().ToListAsync(cancellationToken);

        var consumption = usages
            .GroupBy(u => u.SparePartId)
            .Select(g =>
            {
                var part = allParts.FirstOrDefault(p => p.Id == g.Key);
                return new SparePartConsumptionEntryDto
                {
                    SparePartId = g.Key,
                    PartNumber = part?.PartNumber,
                    Name = part?.Name,
                    QuantityUsed = g.Sum(u => u.Quantity),
                    TotalCost = g.Sum(u => u.TotalAmount),
                    CurrentStock = part?.StockQuantity ?? 0
                };
            })
            .OrderByDescending(c => c.TotalCost)
            .ToList();

        var lowStock = allParts
            .Where(p => p.StockQuantity <= p.ReorderLevel)
            .Select(p => new SparePartDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Category = p.Category,
                UnitCost = p.UnitCost,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                PreferredVendorId = p.PreferredVendorId,
                IsLowStock = true
            })
            .ToList();

        return new SparePartsConsumptionReportDto { Consumption = consumption, LowStockParts = lowStock };
    }
}
