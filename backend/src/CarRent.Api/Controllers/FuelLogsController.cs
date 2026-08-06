using System.Security.Claims;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/fuel-logs")]
[Authorize(Roles = AdminRoles)]
public class FuelLogsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IFuelLogService _fuelLogService;

    public FuelLogsController(IFuelLogService fuelLogService)
    {
        _fuelLogService = fuelLogService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, CancellationToken cancellationToken) =>
        Ok(await _fuelLogService.GetAllAsync(vehicleId, cancellationToken));

    [HttpGet("vehicles/{vehicleId:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid vehicleId, CancellationToken cancellationToken) =>
        Ok(await _fuelLogService.GetConsumptionSummaryAsync(vehicleId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFuelLogRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _fuelLogService.CreateAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
