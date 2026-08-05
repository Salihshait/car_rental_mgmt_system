using CarRent.Application.DTOs.Permissions;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Super Admin,Company Admin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetAllAsync(cancellationToken);
        return Ok(permissions);
    }

    [HttpGet("matrix")]
    public async Task<IActionResult> GetMatrix(CancellationToken cancellationToken)
    {
        var matrix = await _permissionService.GetMatrixAsync(cancellationToken);
        return Ok(matrix);
    }

    [HttpPut("roles/{roleId:guid}")]
    public async Task<IActionResult> UpdateRolePermissions(Guid roleId, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _permissionService.UpdateRolePermissionsAsync(roleId, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
