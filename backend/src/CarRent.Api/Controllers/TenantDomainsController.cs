using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/tenants/{tenantId:guid}/domains")]
[Authorize(Roles = PlatformRoles)]
public class TenantDomainsController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly ITenantDomainService _domainService;

    public TenantDomainsController(ITenantDomainService domainService)
    {
        _domainService = domainService;
    }

    [HttpGet]
    public async Task<IActionResult> GetForTenant(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await _domainService.GetForTenantAsync(tenantId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateTenantDomainRequest request, CancellationToken cancellationToken) =>
        Ok(await _domainService.CreateAsync(tenantId, request, cancellationToken));

    [HttpPost("{domainId:guid}/verify")]
    public async Task<IActionResult> Verify(Guid tenantId, Guid domainId, CancellationToken cancellationToken) =>
        Ok(await _domainService.VerifyAsync(domainId, cancellationToken));
}
