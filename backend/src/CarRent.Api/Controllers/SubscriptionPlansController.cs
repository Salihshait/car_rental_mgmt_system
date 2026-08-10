using CarRent.Application.DTOs.Saas;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/saas/plans")]
public class SubscriptionPlansController : ControllerBase
{
    private const string PlatformRoles = "Platform Owner";

    private readonly ISubscriptionPlanService _planService;

    public SubscriptionPlansController(ISubscriptionPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken cancellationToken) =>
        Ok(await _planService.GetAllAsync(activeOnly, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _planService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = PlatformRoles)]
    public async Task<IActionResult> Create([FromBody] UpsertPlanRequest request, CancellationToken cancellationToken) =>
        Ok(await _planService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = PlatformRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertPlanRequest request, CancellationToken cancellationToken) =>
        Ok(await _planService.UpdateAsync(id, request, cancellationToken));
}
