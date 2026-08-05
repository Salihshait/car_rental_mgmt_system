using CarRent.Application.DTOs.Roles;

namespace CarRent.Application.DTOs.Permissions;

public class PermissionMatrixDto
{
    public List<RoleSummaryDto> Roles { get; set; } = new();
    public List<PermissionDto> Permissions { get; set; } = new();
    public Dictionary<Guid, List<Guid>> RolePermissionIds { get; set; } = new();
}
