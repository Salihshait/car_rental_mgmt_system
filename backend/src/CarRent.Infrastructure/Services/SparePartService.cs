using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class SparePartService : ISparePartService
{
    private readonly CarRentDbContext _context;

    public SparePartService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SparePartDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _context.SpareParts.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return await MapAsync(parts, cancellationToken);
    }

    public async Task<SparePartDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await _context.SpareParts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (part is null)
        {
            return null;
        }

        var mapped = await MapAsync(new List<SparePart> { part }, cancellationToken);
        return mapped.First();
    }

    public async Task<SparePartDto> CreateAsync(SaveSparePartRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.SpareParts.AnyAsync(p => p.PartNumber == request.PartNumber, cancellationToken))
        {
            throw new InvalidOperationException("A spare part with this part number already exists.");
        }

        if (request.PreferredVendorId.HasValue && !await _context.Vendors.AnyAsync(v => v.Id == request.PreferredVendorId, cancellationToken))
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        var part = new SparePart
        {
            Id = Guid.NewGuid(),
            PartNumber = request.PartNumber,
            Name = request.Name,
            Category = request.Category,
            UnitCost = request.UnitCost,
            StockQuantity = request.StockQuantity,
            ReorderLevel = request.ReorderLevel,
            PreferredVendorId = request.PreferredVendorId
        };

        await _context.SpareParts.AddAsync(part, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(part.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created spare part.");
    }

    public async Task<SparePartDto> UpdateAsync(Guid id, SaveSparePartRequest request, CancellationToken cancellationToken = default)
    {
        var part = await _context.SpareParts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Spare part not found.");

        if (await _context.SpareParts.AnyAsync(p => p.Id != id && p.PartNumber == request.PartNumber, cancellationToken))
        {
            throw new InvalidOperationException("Another spare part already uses this part number.");
        }

        part.PartNumber = request.PartNumber;
        part.Name = request.Name;
        part.Category = request.Category;
        part.UnitCost = request.UnitCost;
        part.ReorderLevel = request.ReorderLevel;
        part.PreferredVendorId = request.PreferredVendorId;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated spare part.");
    }

    public async Task<SparePartDto> AdjustStockAsync(Guid id, AdjustStockRequest request, CancellationToken cancellationToken = default)
    {
        var part = await _context.SpareParts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Spare part not found.");

        var newQuantity = part.StockQuantity + request.QuantityChange;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException("Stock quantity cannot go negative.");
        }

        part.StockQuantity = newQuantity;
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated spare part.");
    }

    public async Task<MaintenancePartUsageDto> RecordUsageAsync(Guid maintenanceId, RecordPartUsageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _context.VehicleMaintenances.AnyAsync(m => m.Id == maintenanceId, cancellationToken))
        {
            throw new InvalidOperationException("Maintenance record not found.");
        }

        var part = await _context.SpareParts.FirstOrDefaultAsync(p => p.Id == request.SparePartId, cancellationToken)
            ?? throw new InvalidOperationException("Spare part not found.");

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        if (part.StockQuantity < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for {part.Name}. Available: {part.StockQuantity}.");
        }

        part.StockQuantity -= request.Quantity;

        var usage = new MaintenancePartUsage
        {
            Id = Guid.NewGuid(),
            MaintenanceId = maintenanceId,
            SparePartId = part.Id,
            Quantity = request.Quantity,
            UnitCost = part.UnitCost,
            TotalAmount = Math.Round(part.UnitCost * request.Quantity, 2)
        };

        await _context.MaintenancePartUsages.AddAsync(usage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new MaintenancePartUsageDto
        {
            Id = usage.Id,
            MaintenanceId = usage.MaintenanceId,
            SparePartId = usage.SparePartId,
            SparePartName = part.Name,
            PartNumber = part.PartNumber,
            Quantity = usage.Quantity,
            UnitCost = usage.UnitCost,
            TotalAmount = usage.TotalAmount,
            CreatedAt = usage.CreatedAt
        };
    }

    public async Task<IEnumerable<MaintenancePartUsageDto>> GetUsageAsync(Guid maintenanceId, CancellationToken cancellationToken = default)
    {
        var usages = await _context.MaintenancePartUsages.AsNoTracking().Where(u => u.MaintenanceId == maintenanceId).ToListAsync(cancellationToken);
        var partIds = usages.Select(u => u.SparePartId).Distinct().ToList();
        var parts = await _context.SpareParts.AsNoTracking().Where(p => partIds.Contains(p.Id)).ToListAsync(cancellationToken);

        return usages.Select(u =>
        {
            var part = parts.FirstOrDefault(p => p.Id == u.SparePartId);
            return new MaintenancePartUsageDto
            {
                Id = u.Id,
                MaintenanceId = u.MaintenanceId,
                SparePartId = u.SparePartId,
                SparePartName = part?.Name,
                PartNumber = part?.PartNumber,
                Quantity = u.Quantity,
                UnitCost = u.UnitCost,
                TotalAmount = u.TotalAmount,
                CreatedAt = u.CreatedAt
            };
        }).ToList();
    }

    private async Task<List<SparePartDto>> MapAsync(List<SparePart> parts, CancellationToken cancellationToken)
    {
        var vendorIds = parts.Where(p => p.PreferredVendorId.HasValue).Select(p => p.PreferredVendorId!.Value).Distinct().ToList();
        var vendors = await _context.Vendors.AsNoTracking().Where(v => vendorIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return parts.Select(p =>
        {
            var vendor = p.PreferredVendorId.HasValue ? vendors.FirstOrDefault(v => v.Id == p.PreferredVendorId) : null;
            return new SparePartDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Category = p.Category,
                UnitCost = p.UnitCost,
                StockQuantity = p.StockQuantity,
                ReorderLevel = p.ReorderLevel,
                PreferredVendorId = p.PreferredVendorId,
                PreferredVendorName = vendor?.Name,
                IsLowStock = p.StockQuantity <= p.ReorderLevel
            };
        }).ToList();
    }
}
