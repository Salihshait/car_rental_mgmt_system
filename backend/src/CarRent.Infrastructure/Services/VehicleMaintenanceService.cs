using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleMaintenanceService : IVehicleMaintenanceService
{
    private readonly CarRentDbContext _context;

    public VehicleMaintenanceService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleMaintenanceDto>> GetAllAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleMaintenances.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(m => m.VehicleId == vehicleId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var records = await query.OrderByDescending(m => m.ScheduledOn).ToListAsync(cancellationToken);
        return await MapAsync(records, cancellationToken);
    }

    public async Task<VehicleMaintenanceDto> ScheduleAsync(CreateMaintenanceRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (request.WorkshopId.HasValue && !await _context.Workshops.AnyAsync(w => w.Id == request.WorkshopId, cancellationToken))
        {
            throw new InvalidOperationException("Workshop not found.");
        }

        var maintenance = new VehicleMaintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            WorkshopId = request.WorkshopId,
            MaintenanceType = request.MaintenanceType,
            ServiceType = request.ServiceType,
            ScheduledOn = request.ScheduledOn,
            Status = "Scheduled",
            Cost = request.Cost,
            Notes = request.Notes,
            CreatedBy = createdBy
        };

        await _context.VehicleMaintenances.AddAsync(maintenance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapAsync(maintenance, cancellationToken);
    }

    public async Task<VehicleMaintenanceDto> StartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var maintenance = await _context.VehicleMaintenances.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Maintenance record not found.");

        if (maintenance.Status != "Scheduled")
        {
            throw new InvalidOperationException("Only scheduled maintenance can be started.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == maintenance.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        maintenance.Status = "InProgress";
        vehicle.Status = "Maintenance";
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await MapAsync(maintenance, cancellationToken);
    }

    public async Task<VehicleMaintenanceDto> CompleteAsync(Guid id, CompleteMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        var maintenance = await _context.VehicleMaintenances.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Maintenance record not found.");

        if (maintenance.Status != "InProgress")
        {
            throw new InvalidOperationException("Only in-progress maintenance can be completed.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == maintenance.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        maintenance.Status = "Completed";
        maintenance.CompletedAt = DateTime.UtcNow;
        maintenance.Cost = request.Cost ?? maintenance.Cost;
        maintenance.Notes = request.Notes ?? maintenance.Notes;

        vehicle.Status = "Available";
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await MapAsync(maintenance, cancellationToken);
    }

    public async Task<VehicleMaintenanceDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var maintenance = await _context.VehicleMaintenances.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Maintenance record not found.");

        if (maintenance.Status is "Completed" or "Cancelled")
        {
            throw new InvalidOperationException("This maintenance record is already closed.");
        }

        var wasInProgress = maintenance.Status == "InProgress";
        maintenance.Status = "Cancelled";

        if (wasInProgress)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == maintenance.VehicleId, cancellationToken);
            if (vehicle is not null)
            {
                vehicle.Status = "Available";
                vehicle.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await MapAsync(maintenance, cancellationToken);
    }

    private async Task<VehicleMaintenanceDto> MapAsync(VehicleMaintenance maintenance, CancellationToken cancellationToken)
    {
        var mapped = await MapAsync(new List<VehicleMaintenance> { maintenance }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<VehicleMaintenanceDto>> MapAsync(List<VehicleMaintenance> records, CancellationToken cancellationToken)
    {
        var vehicleIds = records.Select(m => m.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var workshopIds = records.Where(m => m.WorkshopId.HasValue).Select(m => m.WorkshopId!.Value).Distinct().ToList();
        var workshops = await _context.Workshops.AsNoTracking().Where(w => workshopIds.Contains(w.Id)).ToListAsync(cancellationToken);

        return records.Select(m =>
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == m.VehicleId);
            var workshop = m.WorkshopId.HasValue ? workshops.FirstOrDefault(w => w.Id == m.WorkshopId) : null;

            return new VehicleMaintenanceDto
            {
                Id = m.Id,
                VehicleId = m.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                WorkshopId = m.WorkshopId,
                WorkshopName = workshop?.Name,
                MaintenanceType = m.MaintenanceType,
                ServiceType = m.ServiceType,
                ScheduledOn = m.ScheduledOn,
                Status = m.Status,
                Cost = m.Cost,
                CompletedAt = m.CompletedAt,
                Notes = m.Notes,
                CreatedAt = m.CreatedAt
            };
        }).ToList();
    }
}
