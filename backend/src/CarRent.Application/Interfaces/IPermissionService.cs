using CarRent.Application.DTOs.Permissions;

namespace CarRent.Application.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PermissionMatrixDto> GetMatrixAsync(CancellationToken cancellationToken = default);
    Task UpdateRolePermissionsAsync(Guid roleId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
}
