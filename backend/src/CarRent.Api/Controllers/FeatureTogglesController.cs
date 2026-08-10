using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants/{tenantId:guid}/feature-toggles")]
[Authorize(Roles = PlatformRoles)]
public class FeatureTogglesController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly IFeatureToggleService _featureToggleService;

    public FeatureTogglesController(IFeatureToggleService featureToggleService)
    {
        _featureToggleService = featureToggleService;
    }

    [HttpGet("overrides")]
    public async Task<IActionResult> GetOverrides(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await _featureToggleService.GetTenantOverridesAsync(tenantId, cancellationToken));

    [HttpPut("overrides")]
    public async Task<IActionResult> UpsertOverride(Guid tenantId, [FromBody] UpsertTenantFeatureOverrideRequest request, CancellationToken cancellationToken) =>
        Ok(await _featureToggleService.UpsertTenantOverrideAsync(tenantId, request, cancellationToken));

    [HttpGet("resolve/{featureKey}")]
    public async Task<IActionResult> Resolve(Guid tenantId, string featureKey, CancellationToken cancellationToken) =>
        Ok(await _featureToggleService.ResolveAsync(tenantId, featureKey, cancellationToken));
}
