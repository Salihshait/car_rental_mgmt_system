using CarRent.Application.DTOs.Permissions;
using CarRent.Application.DTOs.Roles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly CarRentDbContext _context;

    public PermissionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionDto { Id = p.Id, Name = p.Name, Description = p.Description })
            .ToListAsync(cancellationToken);
    }

    public async Task<PermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleSummaryDto { Id = r.Id, Name = r.Name, Description = r.Description, IsSystem = r.IsSystem })
            .ToListAsync(cancellationToken);

        var permissions = await GetAllAsync(cancellationToken);

        var assignments = await _context.RolePermissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rolePermissionIds = roles.ToDictionary(
            role => role.Id,
            role => assignments.Where(a => a.RoleId == role.Id).Select(a => a.PermissionId).ToList());

        return new PermissionMatrixDto
        {
            Roles = roles,
            Permissions = permissions.ToList(),
            RolePermissionIds = rolePermissionIds
        };
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);
        if (!roleExists)
        {
            throw new InvalidOperationException("Role not found.");
        }

        var validPermissionIds = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in validPermissionIds)
        {
            await _context.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
