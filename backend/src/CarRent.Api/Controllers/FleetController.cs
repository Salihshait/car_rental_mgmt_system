using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/fleet")]
[Authorize(Roles = AdminRoles)]
public class FleetController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IFleetService _fleetService;

    public FleetController(IFleetService fleetService)
    {
        _fleetService = fleetService;
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(CancellationToken cancellationToken) =>
        Ok(await _fleetService.GetAvailabilityAsync(cancellationToken));

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await _fleetService.GetDashboardAsync(cancellationToken));
}
