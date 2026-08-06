using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class FuelLogService : IFuelLogService
{
    private readonly CarRentDbContext _context;

    public FuelLogService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FuelLogDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.FuelLogs.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(f => f.VehicleId == vehicleId);
        }

        return await query
            .Join(_context.Vehicles, f => f.VehicleId, v => v.Id, (f, v) => new FuelLogDto
            {
                Id = f.Id,
                VehicleId = f.VehicleId,
                VehicleRegistrationNumber = v.RegistrationNumber,
                LoggedOn = f.LoggedOn,
                Quantity = f.Quantity,
                Cost = f.Cost,
                OdometerReading = f.OdometerReading,
                LogType = f.LogType
            })
            .OrderByDescending(f => f.LoggedOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<FuelLogDto> CreateAsync(CreateFuelLogRequest request, Guid recordedBy, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var log = new FuelLog
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            LoggedOn = request.LoggedOn ?? DateTime.UtcNow,
            Quantity = request.Quantity,
            Cost = request.Cost,
            OdometerReading = request.OdometerReading,
            LogType = request.LogType,
            RecordedBy = recordedBy
        };

        await _context.FuelLogs.AddAsync(log, cancellationToken);

        if (request.OdometerReading.HasValue && request.OdometerReading > (vehicle.CurrentOdometerReading ?? 0))
        {
            vehicle.CurrentOdometerReading = request.OdometerReading;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new FuelLogDto
        {
            Id = log.Id,
            VehicleId = log.VehicleId,
            VehicleRegistrationNumber = vehicle.RegistrationNumber,
            LoggedOn = log.LoggedOn,
            Quantity = log.Quantity,
            Cost = log.Cost,
            OdometerReading = log.OdometerReading,
            LogType = log.LogType
        };
    }

    public async Task<FuelConsumptionSummaryDto> GetConsumptionSummaryAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.FuelLogs.AsNoTracking().Where(f => f.VehicleId == vehicleId).ToListAsync(cancellationToken);

        var odometerLogs = logs.Where(l => l.OdometerReading.HasValue).ToList();
        decimal? distancePerUnit = null;

        if (odometerLogs.Count >= 2)
        {
            var minOdometer = odometerLogs.Min(l => l.OdometerReading!.Value);
            var maxOdometer = odometerLogs.Max(l => l.OdometerReading!.Value);
            var totalQuantity = logs.Sum(l => l.Quantity);

            if (totalQuantity > 0 && maxOdometer > minOdometer)
            {
                distancePerUnit = Math.Round((maxOdometer - minOdometer) / totalQuantity, 2);
            }
        }

        return new FuelConsumptionSummaryDto
        {
            VehicleId = vehicleId,
            TotalQuantity = logs.Sum(l => l.Quantity),
            TotalCost = logs.Sum(l => l.Cost),
            LogCount = logs.Count,
            DistancePerUnit = distancePerUnit
        };
    }
}
