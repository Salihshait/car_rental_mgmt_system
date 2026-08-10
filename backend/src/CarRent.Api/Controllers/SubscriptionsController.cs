using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants/{tenantId:guid}/subscriptions")]
[Authorize(Roles = PlatformRoles)]
public class SubscriptionsController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetForTenant(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await _subscriptionService.GetForTenantAsync(tenantId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken) =>
        Ok(await _subscriptionService.CreateAsync(tenantId, request, cancellationToken));

    [HttpPost("{subscriptionId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid tenantId, Guid subscriptionId, CancellationToken cancellationToken) =>
        Ok(await _subscriptionService.CancelAsync(subscriptionId, cancellationToken));
}
