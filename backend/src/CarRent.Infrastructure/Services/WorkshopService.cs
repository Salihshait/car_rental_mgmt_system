using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class WorkshopService : IWorkshopService
{
    private readonly CarRentDbContext _context;

    public WorkshopService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkshopDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().OrderBy(w => w.Name).ToListAsync(cancellationToken);
    }

    public async Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkshopDto> CreateAsync(SaveWorkshopRequest request, CancellationToken cancellationToken = default)
    {
        if (request.VendorId.HasValue && !await _context.Vendors.AnyAsync(v => v.Id == request.VendorId, cancellationToken))
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            VendorId = request.VendorId,
            Address = request.Address,
            Phone = request.Phone,
            IsActive = request.IsActive
        };

        await _context.Workshops.AddAsync(workshop, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(workshop.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created workshop.");
    }

    public async Task<WorkshopDto> UpdateAsync(Guid id, SaveWorkshopRequest request, CancellationToken cancellationToken = default)
    {
        var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Workshop not found.");

        if (request.VendorId.HasValue && !await _context.Vendors.AnyAsync(v => v.Id == request.VendorId, cancellationToken))
        {
            throw new InvalidOperationException("Vendor not found.");
        }

        workshop.Name = request.Name;
        workshop.VendorId = request.VendorId;
        workshop.Address = request.Address;
        workshop.Phone = request.Phone;
        workshop.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated workshop.");
    }

    private IQueryable<WorkshopDto> Query()
    {
        return _context.Workshops
            .AsNoTracking()
            .GroupJoin(_context.Vendors, w => w.VendorId, v => v.Id, (w, vendors) => new { w, Vendor = vendors.FirstOrDefault() })
            .Select(x => new WorkshopDto
            {
                Id = x.w.Id,
                Name = x.w.Name,
                VendorId = x.w.VendorId,
                VendorName = x.Vendor != null ? x.Vendor.Name : null,
                Address = x.w.Address,
                Phone = x.w.Phone,
                IsActive = x.w.IsActive,
                CreatedAt = x.w.CreatedAt
            });
    }
}
