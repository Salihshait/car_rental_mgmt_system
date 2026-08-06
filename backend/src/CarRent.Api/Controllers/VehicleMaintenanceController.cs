using System.Security.Claims;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/vehicle-maintenance")]
[Authorize(Roles = AdminRoles)]
public class VehicleMaintenanceController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleMaintenanceService _maintenanceService;
    private readonly ISparePartService _sparePartService;

    public VehicleMaintenanceController(IVehicleMaintenanceService maintenanceService, ISparePartService sparePartService)
    {
        _maintenanceService = maintenanceService;
        _sparePartService = sparePartService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, [FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _maintenanceService.GetAllAsync(vehicleId, status, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Schedule([FromBody] CreateMaintenanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _maintenanceService.ScheduleAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _maintenanceService.StartAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteMaintenanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _maintenanceService.CompleteAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _maintenanceService.CancelAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/parts")]
    public async Task<IActionResult> GetParts(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sparePartService.GetUsageAsync(id, cancellationToken));

    [HttpPost("{id:guid}/parts")]
    public async Task<IActionResult> RecordPartUsage(Guid id, [FromBody] RecordPartUsageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _sparePartService.RecordUsageAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
