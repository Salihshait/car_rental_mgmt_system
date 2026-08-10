using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants/{tenantId:guid}/branding")]
[Authorize(Roles = PlatformRoles)]
public class TenantBrandingController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly ITenantBrandingService _brandingService;

    public TenantBrandingController(ITenantBrandingService brandingService)
    {
        _brandingService = brandingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await _brandingService.GetAsync(tenantId, cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Upsert(Guid tenantId, [FromBody] UpsertTenantBrandingRequest request, CancellationToken cancellationToken) =>
        Ok(await _brandingService.UpsertAsync(tenantId, request, cancellationToken));
}
