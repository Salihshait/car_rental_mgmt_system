using CarRent.Application.DTOs.Roles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly CarRentDbContext _context;

    public RoleService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public async Task<RoleSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<RoleSummaryDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var nameInUse = await _context.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken);
        if (nameInUse)
        {
            throw new InvalidOperationException("A role with this name already exists.");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystem = false
        };

        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(role.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created role.");
    }

    public async Task<RoleSummaryDto> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Role not found.");

        if (role.IsSystem)
        {
            throw new InvalidOperationException("System roles cannot be modified.");
        }

        role.Name = request.Name;
        role.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated role.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Role not found.");

        if (role.IsSystem)
        {
            throw new InvalidOperationException("System roles cannot be deleted.");
        }

        var inUse = await _context.Users.AnyAsync(u => u.RoleId == id, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("This role is assigned to one or more users and cannot be deleted.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<RoleSummaryDto> Query()
    {
        return _context.Roles
            .AsNoTracking()
            .Select(r => new RoleSummaryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystem = r.IsSystem
            });
    }
}
