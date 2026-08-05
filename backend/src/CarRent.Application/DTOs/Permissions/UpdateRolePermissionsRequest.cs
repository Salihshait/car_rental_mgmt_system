namespace CarRent.Application.DTOs.Permissions;

public class UpdateRolePermissionsRequest
{
    public List<Guid> PermissionIds { get; set; } = new();
}
