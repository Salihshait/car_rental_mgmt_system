using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverAssignmentService : IDriverAssignmentService
{
    private readonly CarRentDbContext _context;

    public DriverAssignmentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DriverAssignmentDto>> GetHistoryAsync(Guid? vehicleId, Guid? driverId, CancellationToken cancellationToken = default)
    {
        var query = _context.DriverAssignments.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(a => a.VehicleId == vehicleId);
        }

        if (driverId.HasValue)
        {
            query = query.Where(a => a.DriverId == driverId);
        }

        var assignments = await query.OrderByDescending(a => a.AssignedAt).ToListAsync(cancellationToken);
        return await MapAsync(assignments, cancellationToken);
    }

    public async Task<DriverAssignmentDto> AssignAsync(AssignDriverRequest request, Guid assignedBy, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken)
            ?? throw new InvalidOperationException("Driver not found.");

        var existingActive = await _context.DriverAssignments
            .Where(a => a.VehicleId == vehicle.Id && a.UnassignedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var previous in existingActive)
        {
            previous.UnassignedAt = DateTime.UtcNow;
        }

        var assignment = new DriverAssignment
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            DriverId = driver.Id,
            AssignedBy = assignedBy,
            Notes = request.Notes
        };

        await _context.DriverAssignments.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<DriverAssignment> { assignment }, cancellationToken);
        return mapped.First();
    }

    public async Task UnassignAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.DriverAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken)
            ?? throw new InvalidOperationException("Assignment not found.");

        if (assignment.UnassignedAt is not null)
        {
            throw new InvalidOperationException("This assignment has already ended.");
        }

        assignment.UnassignedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<DriverAssignmentDto>> MapAsync(List<DriverAssignment> assignments, CancellationToken cancellationToken)
    {
        var vehicleIds = assignments.Select(a => a.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var driverIds = assignments.Select(a => a.DriverId).Distinct().ToList();
        var drivers = await _context.Drivers.AsNoTracking().Where(d => driverIds.Contains(d.Id)).ToListAsync(cancellationToken);
        var driverUserIds = drivers.Select(d => d.UserId).ToList();
        var driverUsers = await _context.Users.AsNoTracking().Where(u => driverUserIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return assignments.Select(a =>
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == a.VehicleId);
            var driver = drivers.FirstOrDefault(d => d.Id == a.DriverId);
            var driverUser = driver is null ? null : driverUsers.FirstOrDefault(u => u.Id == driver.UserId);

            return new DriverAssignmentDto
            {
                Id = a.Id,
                VehicleId = a.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                DriverId = a.DriverId,
                DriverName = driverUser is null ? null : $"{driverUser.FirstName} {driverUser.LastName}",
                AssignedAt = a.AssignedAt,
                UnassignedAt = a.UnassignedAt,
                Notes = a.Notes
            };
        }).ToList();
    }
}
