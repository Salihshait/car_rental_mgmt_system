using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly CarRentDbContext _context;

    public VendorService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().OrderBy(v => v.Name).ToListAsync(cancellationToken);
    }

    public async Task<VendorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<VendorDto> CreateAsync(SaveVendorRequest request, CancellationToken cancellationToken = default)
    {
        var vendor = new Vendor
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            VendorType = request.VendorType,
            ContactName = request.ContactName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            IsActive = request.IsActive
        };

        await _context.Vendors.AddAsync(vendor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(vendor.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created vendor.");
    }

    public async Task<VendorDto> UpdateAsync(Guid id, SaveVendorRequest request, CancellationToken cancellationToken = default)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Vendor not found.");

        vendor.Name = request.Name;
        vendor.VendorType = request.VendorType;
        vendor.ContactName = request.ContactName;
        vendor.Phone = request.Phone;
        vendor.Email = request.Email;
        vendor.Address = request.Address;
        vendor.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated vendor.");
    }

    private IQueryable<VendorDto> Query()
    {
        return _context.Vendors.AsNoTracking().Select(v => new VendorDto
        {
            Id = v.Id,
            Name = v.Name,
            VendorType = v.VendorType,
            ContactName = v.ContactName,
            Phone = v.Phone,
            Email = v.Email,
            Address = v.Address,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        });
    }
}
