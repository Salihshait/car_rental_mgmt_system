using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleWarrantyService : IVehicleWarrantyService
{
    private readonly CarRentDbContext _context;

    public VehicleWarrantyService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleWarrantyDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleWarranties.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(w => w.VehicleId == vehicleId);
        }

        var warranties = await query.OrderByDescending(w => w.EndDate).ToListAsync(cancellationToken);
        var vehicleIds = warranties.Select(w => w.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return warranties.Select(w => new VehicleWarrantyDto
        {
            Id = w.Id,
            VehicleId = w.VehicleId,
            VehicleRegistrationNumber = vehicles.FirstOrDefault(v => v.Id == w.VehicleId)?.RegistrationNumber,
            WarrantyType = w.WarrantyType,
            Provider = w.Provider,
            StartDate = w.StartDate,
            EndDate = w.EndDate,
            CoverageDetails = w.CoverageDetails,
            Status = MaintenanceStatusHelper.ComputeExpiryStatus(w.Status, w.EndDate)
        }).ToList();
    }

    public async Task<VehicleWarrantyDto> CreateAsync(CreateVehicleWarrantyRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidOperationException("End date must be after the start date.");
        }

        var warranty = new VehicleWarranty
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            WarrantyType = request.WarrantyType,
            Provider = request.Provider,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CoverageDetails = request.CoverageDetails
        };

        await _context.VehicleWarranties.AddAsync(warranty, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleWarrantyDto
        {
            Id = warranty.Id,
            VehicleId = warranty.VehicleId,
            VehicleRegistrationNumber = vehicle.RegistrationNumber,
            WarrantyType = warranty.WarrantyType,
            Provider = warranty.Provider,
            StartDate = warranty.StartDate,
            EndDate = warranty.EndDate,
            CoverageDetails = warranty.CoverageDetails,
            Status = MaintenanceStatusHelper.ComputeExpiryStatus(warranty.Status, warranty.EndDate)
        };
    }
}
