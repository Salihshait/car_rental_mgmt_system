using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleTransferService : IVehicleTransferService
{
    private readonly CarRentDbContext _context;

    public VehicleTransferService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleTransferDto>> GetAllAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.VehicleTransfers.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(t => t.VehicleId == vehicleId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var transfers = await query.OrderByDescending(t => t.RequestedAt).ToListAsync(cancellationToken);
        return await MapAsync(transfers, cancellationToken);
    }

    public async Task<VehicleTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedBy, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        if (!await _context.Branches.AnyAsync(b => b.Id == request.ToBranchId, cancellationToken))
        {
            throw new InvalidOperationException("Destination branch not found.");
        }

        if (vehicle.BranchId == request.ToBranchId)
        {
            throw new InvalidOperationException("The vehicle is already at that branch.");
        }

        if (await _context.VehicleTransfers.AnyAsync(t => t.VehicleId == vehicle.Id && t.Status == "InTransit", cancellationToken))
        {
            throw new InvalidOperationException("This vehicle already has a transfer in progress.");
        }

        var transfer = new VehicleTransfer
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            FromBranchId = vehicle.BranchId,
            ToBranchId = request.ToBranchId,
            RequestedBy = requestedBy,
            Status = "InTransit",
            Notes = request.Notes
        };

        await _context.VehicleTransfers.AddAsync(transfer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<VehicleTransfer> { transfer }, cancellationToken);
        return mapped.First();
    }

    public async Task<VehicleTransferDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await _context.VehicleTransfers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Transfer not found.");

        if (transfer.Status != "InTransit")
        {
            throw new InvalidOperationException("Only in-transit transfers can be completed.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == transfer.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        transfer.Status = "Completed";
        transfer.CompletedAt = DateTime.UtcNow;

        vehicle.BranchId = transfer.ToBranchId;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<VehicleTransfer> { transfer }, cancellationToken);
        return mapped.First();
    }

    public async Task<VehicleTransferDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transfer = await _context.VehicleTransfers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Transfer not found.");

        if (transfer.Status != "InTransit")
        {
            throw new InvalidOperationException("Only in-transit transfers can be cancelled.");
        }

        transfer.Status = "Cancelled";
        transfer.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<VehicleTransfer> { transfer }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<VehicleTransferDto>> MapAsync(List<VehicleTransfer> transfers, CancellationToken cancellationToken)
    {
        var vehicleIds = transfers.Select(t => t.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var branchIds = transfers.Select(t => t.FromBranchId).Concat(transfers.Select(t => t.ToBranchId)).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(b => branchIds.Contains(b.Id)).ToListAsync(cancellationToken);

        return transfers.Select(t =>
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == t.VehicleId);
            var fromBranch = branches.FirstOrDefault(b => b.Id == t.FromBranchId);
            var toBranch = branches.FirstOrDefault(b => b.Id == t.ToBranchId);

            return new VehicleTransferDto
            {
                Id = t.Id,
                VehicleId = t.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                FromBranchId = t.FromBranchId,
                FromBranchName = fromBranch?.Name,
                ToBranchId = t.ToBranchId,
                ToBranchName = toBranch?.Name,
                Status = t.Status,
                RequestedAt = t.RequestedAt,
                CompletedAt = t.CompletedAt,
                Notes = t.Notes
            };
        }).ToList();
    }
}
