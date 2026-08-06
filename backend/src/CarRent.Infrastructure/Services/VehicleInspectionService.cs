using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleInspectionService : IVehicleInspectionService
{
    private readonly CarRentDbContext _context;

    public VehicleInspectionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleInspectionDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleInspections.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(i => i.VehicleId == vehicleId);
        }

        var inspections = await query.OrderByDescending(i => i.InspectionDate).ToListAsync(cancellationToken);
        return await MapAsync(inspections, cancellationToken);
    }

    public async Task<VehicleInspectionDto> CreateAsync(CreateVehicleInspectionRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId, cancellationToken))
        {
            throw new InvalidOperationException("Vehicle not found.");
        }

        if (request.VendorId.HasValue && !await _context.Vendors.AnyAsync(v => v.Id == request.VendorId, cancellationToken))
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        var inspection = new VehicleInspection
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            InspectionType = request.InspectionType,
            InspectionDate = request.InspectionDate,
            NextDueDate = request.NextDueDate,
            Result = request.Result,
            InspectorName = request.InspectorName,
            VendorId = request.VendorId,
            Notes = request.Notes,
            CertificateUrl = request.CertificateUrl
        };

        await _context.VehicleInspections.AddAsync(inspection, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<VehicleInspection> { inspection }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<VehicleInspectionDto>> MapAsync(List<VehicleInspection> inspections, CancellationToken cancellationToken)
    {
        var vehicleIds = inspections.Select(i => i.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var vendorIds = inspections.Where(i => i.VendorId.HasValue).Select(i => i.VendorId!.Value).Distinct().ToList();
        var vendors = await _context.Vendors.AsNoTracking().Where(v => vendorIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return inspections.Select(i => new VehicleInspectionDto
        {
            Id = i.Id,
            VehicleId = i.VehicleId,
            VehicleRegistrationNumber = vehicles.FirstOrDefault(v => v.Id == i.VehicleId)?.RegistrationNumber,
            InspectionType = i.InspectionType,
            InspectionDate = i.InspectionDate,
            NextDueDate = i.NextDueDate,
            Result = i.Result,
            InspectorName = i.InspectorName,
            VendorId = i.VendorId,
            VendorName = i.VendorId.HasValue ? vendors.FirstOrDefault(v => v.Id == i.VendorId)?.Name : null,
            Notes = i.Notes,
            CertificateUrl = i.CertificateUrl
        }).ToList();
    }
}
