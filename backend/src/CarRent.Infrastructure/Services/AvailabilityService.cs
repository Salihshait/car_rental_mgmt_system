using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    public static readonly string[] BlockingStatuses = { "Cancelled", "Rejected", "NoShow" };

    private readonly CarRentDbContext _context;

    public AvailabilityService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsVehicleAvailableAsync(Guid vehicleId, DateTime startDate, DateTime endDate, Guid? excludeBookingId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.VehicleId == vehicleId
                && !BlockingStatuses.Contains(b.Status)
                && startDate < b.EndDate && endDate > b.StartDate);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<VehicleAvailabilityDto> GetVehicleAvailabilityAsync(Guid vehicleId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var bookedRanges = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.VehicleId == vehicleId
                && !BlockingStatuses.Contains(b.Status)
                && from < b.EndDate && to > b.StartDate)
            .Select(b => new BookedRangeDto { BookingId = b.Id, StartDate = b.StartDate, EndDate = b.EndDate, Status = b.Status })
            .ToListAsync(cancellationToken);

        return new VehicleAvailabilityDto
        {
            VehicleId = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            BookedRanges = bookedRanges
        };
    }

    public async Task<IEnumerable<VehicleAvailabilityDto>> GetAvailabilityCalendarAsync(Guid? branchId, Guid? categoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var vehicleQuery = _context.Vehicles.AsNoTracking();

        if (branchId.HasValue)
        {
            vehicleQuery = vehicleQuery.Where(v => v.BranchId == branchId);
        }

        if (categoryId.HasValue)
        {
            vehicleQuery = vehicleQuery.Where(v => v.VehicleModel != null && v.VehicleModel.CategoryId == categoryId);
        }

        var vehicles = await vehicleQuery
            .Select(v => new { v.Id, v.RegistrationNumber })
            .ToListAsync(cancellationToken);

        var vehicleIds = vehicles.Select(v => v.Id).ToList();

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(b => vehicleIds.Contains(b.VehicleId)
                && !BlockingStatuses.Contains(b.Status)
                && from < b.EndDate && to > b.StartDate)
            .Select(b => new { b.Id, b.VehicleId, b.StartDate, b.EndDate, b.Status })
            .ToListAsync(cancellationToken);

        return vehicles.Select(v => new VehicleAvailabilityDto
        {
            VehicleId = v.Id,
            RegistrationNumber = v.RegistrationNumber,
            BookedRanges = bookings
                .Where(b => b.VehicleId == v.Id)
                .Select(b => new BookedRangeDto { BookingId = b.Id, StartDate = b.StartDate, EndDate = b.EndDate, Status = b.Status })
                .ToList()
        });
    }
}
