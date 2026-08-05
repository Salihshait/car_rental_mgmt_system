using CarRent.Application.DTOs.Roles;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin";

    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetByIdAsync(id, cancellationToken);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _roleService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _roleService.UpdateAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
