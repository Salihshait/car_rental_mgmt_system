using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants/{tenantId:guid}/plan-limits")]
[Authorize(Roles = PlatformRoles)]
public class PlanLimitsController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly IPlanLimitService _planLimitService;

    public PlanLimitsController(IPlanLimitService planLimitService)
    {
        _planLimitService = planLimitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEffectiveLimits(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await _planLimitService.GetEffectiveLimitsAsync(tenantId, cancellationToken));
}
