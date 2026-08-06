using System.Security.Claims;
using CarRent.Application.DTOs.Waitlist;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WaitlistController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IWaitlistService _waitlistService;

    public WaitlistController(IWaitlistService waitlistService)
    {
        _waitlistService = waitlistService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private bool IsAdmin => User.IsInRole("Super Admin") || User.IsInRole("Company Admin") || User.IsInRole("Branch Manager");

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] string? status,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? vehicleCategoryId,
        CancellationToken cancellationToken)
    {
        var filter = new WaitlistFilter { CustomerId = customerId, Status = status, BranchId = branchId, VehicleCategoryId = vehicleCategoryId };
        return Ok(await _waitlistService.GetAllAsync(filter, cancellationToken));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var filter = new WaitlistFilter { CustomerId = CurrentUserId };
        return Ok(await _waitlistService.GetAllAsync(filter, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWaitlistRequest request, CancellationToken cancellationToken)
    {
        if (request.CustomerId.HasValue && request.CustomerId != CurrentUserId && !IsAdmin)
        {
            return Forbid();
        }

        try
        {
            var created = await _waitlistService.CreateAsync(request.CustomerId ?? CurrentUserId, request, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _waitlistService.CancelAsync(id, CurrentUserId, IsAdmin, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
