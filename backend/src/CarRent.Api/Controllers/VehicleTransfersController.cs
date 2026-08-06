using System.Security.Claims;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/vehicle-transfers")]
[Authorize(Roles = AdminRoles)]
public class VehicleTransfersController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleTransferService _transferService;

    public VehicleTransfersController(IVehicleTransferService transferService)
    {
        _transferService = transferService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, [FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _transferService.GetAllAsync(vehicleId, status, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _transferService.CreateAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _transferService.CompleteAsync(id, cancellationToken));
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
            return Ok(await _transferService.CancelAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
