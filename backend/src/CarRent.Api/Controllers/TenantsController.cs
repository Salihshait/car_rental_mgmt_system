using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants")]
[Authorize(Roles = PlatformRoles)]
public class TenantsController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterTenantRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _tenantService.RegisterAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _tenantService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _tenantService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken) =>
        Ok(await _tenantService.UpdateAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/database-info")]
    public async Task<IActionResult> GetDatabaseInfo(Guid id, CancellationToken cancellationToken) =>
        Ok(await _tenantService.GetDatabaseInfoAsync(id, cancellationToken));
}
