using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/usage")]
[Authorize(Roles = PlatformRoles)]
public class UsageMonitoringController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly IUsageMonitoringService _usageMonitoringService;

    public UsageMonitoringController(IUsageMonitoringService usageMonitoringService)
    {
        _usageMonitoringService = usageMonitoringService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetPlatformOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _usageMonitoringService.GetPlatformOverviewAsync(from, to, cancellationToken));

    [HttpGet("tenants/{tenantId:guid}")]
    public async Task<IActionResult> GetTenantUsage(Guid tenantId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _usageMonitoringService.GetTenantUsageAsync(tenantId, from, to, cancellationToken));

    [HttpPost("tenants/{tenantId:guid}")]
    public async Task<IActionResult> RecordMetric(Guid tenantId, [FromBody] RecordUsageMetricRequest request, CancellationToken cancellationToken)
    {
        await _usageMonitoringService.RecordMetricAsync(tenantId, request, cancellationToken);
        return NoContent();
    }
}
