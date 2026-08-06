using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class FleetService : IFleetService
{
    private readonly CarRentDbContext _context;

    public FleetService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FleetAvailabilityDto>> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _context.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
        var branches = await _context.Branches.AsNoTracking().ToListAsync(cancellationToken);

        var activeBookingVehicleIds = await (
            from r in _context.Rentals.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
            where r.Status == "Active"
            select b.VehicleId).ToListAsync(cancellationToken);

        var inMaintenanceVehicleIds = await _context.VehicleMaintenances.AsNoTracking()
            .Where(m => m.Status == "InProgress")
            .Select(m => m.VehicleId)
            .ToListAsync(cancellationToken);

        var inTransitVehicleIds = await _context.VehicleTransfers.AsNoTracking()
            .Where(t => t.Status == "InTransit")
            .Select(t => t.VehicleId)
            .ToListAsync(cancellationToken);

        return vehicles.Select(v => new FleetAvailabilityDto
        {
            VehicleId = v.Id,
            RegistrationNumber = v.RegistrationNumber,
            BranchId = v.BranchId,
            BranchName = branches.FirstOrDefault(b => b.Id == v.BranchId)?.Name,
            VehicleStatus = v.Status,
            FleetAvailabilityStatus = FleetStatusHelper.Compute(
                v.Status,
                activeBookingVehicleIds.Contains(v.Id),
                inMaintenanceVehicleIds.Contains(v.Id),
                inTransitVehicleIds.Contains(v.Id))
        }).ToList();
    }

    public async Task<FleetDashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _context.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
        var total = vehicles.Count;
        var available = vehicles.Count(v => v.Status == "Available");

        var now = DateTime.UtcNow;
        var maintenanceDueSoon = await _context.VehicleMaintenances.AsNoTracking()
            .CountAsync(m => m.Status == "Scheduled" && m.ScheduledOn >= now && m.ScheduledOn <= now.AddDays(7), cancellationToken);
        var maintenanceOverdue = await _context.VehicleMaintenances.AsNoTracking()
            .CountAsync(m => m.Status == "Scheduled" && m.ScheduledOn < now, cancellationToken);

        var activeTripCount = await _context.Trips.AsNoTracking().CountAsync(t => t.Status == "InProgress", cancellationToken);

        var gpsThreshold = now.AddMinutes(-30);
        var vehiclesReportingGps = await _context.VehicleLocations.AsNoTracking()
            .Where(l => l.RecordedAt >= gpsThreshold)
            .Select(l => l.VehicleId)
            .Distinct()
            .CountAsync(cancellationToken);

        var activeDriverAssignments = await _context.DriverAssignments.AsNoTracking()
            .CountAsync(a => a.UnassignedAt == null, cancellationToken);

        var inTransitTransfers = await _context.VehicleTransfers.AsNoTracking()
            .CountAsync(t => t.Status == "InTransit", cancellationToken);

        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fuelCostThisMonth = await _context.FuelLogs.AsNoTracking()
            .Where(f => f.LoggedOn >= monthStart)
            .SumAsync(f => (decimal?)f.Cost, cancellationToken) ?? 0;

        return new FleetDashboardSummaryDto
        {
            TotalVehicles = total,
            AvailableVehicles = available,
            UtilizationPercent = total == 0 ? 0 : Math.Round((double)(total - available) / total * 100, 1),
            StatusCounts = vehicles.GroupBy(v => v.Status).ToDictionary(g => g.Key, g => g.Count()),
            MaintenanceDueCount = maintenanceDueSoon,
            MaintenanceOverdueCount = maintenanceOverdue,
            ActiveTripCount = activeTripCount,
            VehiclesReportingGpsCount = vehiclesReportingGps,
            ActiveDriverAssignmentCount = activeDriverAssignments,
            InTransitTransferCount = inTransitTransfers,
            FuelCostThisMonth = fuelCostThisMonth
        };
    }
}
