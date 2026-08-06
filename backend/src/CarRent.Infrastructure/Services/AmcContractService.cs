using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class AmcContractService : IAmcContractService
{
    private readonly CarRentDbContext _context;

    public AmcContractService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AmcContractDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.AmcContracts.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(c => c.VehicleId == vehicleId);
        }

        var contracts = await query.OrderByDescending(c => c.EndDate).ToListAsync(cancellationToken);
        return await MapAsync(contracts, cancellationToken);
    }

    public async Task<AmcContractDto> CreateAsync(CreateAmcContractRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId, cancellationToken))
        {
            throw new InvalidOperationException("Vehicle not found.");
        }

        if (!await _context.Vendors.AnyAsync(v => v.Id == request.VendorId, cancellationToken))
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidOperationException("End date must be after the start date.");
        }

        var contract = new AmcContract
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            VendorId = request.VendorId,
            ContractNumber = request.ContractNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CoverageDetails = request.CoverageDetails,
            Cost = request.Cost
        };

        await _context.AmcContracts.AddAsync(contract, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<AmcContract> { contract }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<AmcContractDto>> MapAsync(List<AmcContract> contracts, CancellationToken cancellationToken)
    {
        var vehicleIds = contracts.Select(c => c.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var vendorIds = contracts.Select(c => c.VendorId).Distinct().ToList();
        var vendors = await _context.Vendors.AsNoTracking().Where(v => vendorIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return contracts.Select(c => new AmcContractDto
        {
            Id = c.Id,
            VehicleId = c.VehicleId,
            VehicleRegistrationNumber = vehicles.FirstOrDefault(v => v.Id == c.VehicleId)?.RegistrationNumber,
            VendorId = c.VendorId,
            VendorName = vendors.FirstOrDefault(v => v.Id == c.VendorId)?.Name,
            ContractNumber = c.ContractNumber,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            CoverageDetails = c.CoverageDetails,
            Cost = c.Cost,
            Status = MaintenanceStatusHelper.ComputeExpiryStatus(c.Status, c.EndDate)
        }).ToList();
    }
}
