using System.Security.Claims;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/driver-assignments")]
[Authorize(Roles = AdminRoles)]
public class DriverAssignmentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDriverAssignmentService _assignmentService;

    public DriverAssignmentsController(IDriverAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? vehicleId, [FromQuery] Guid? driverId, CancellationToken cancellationToken) =>
        Ok(await _assignmentService.GetHistoryAsync(vehicleId, driverId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] AssignDriverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _assignmentService.AssignAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/unassign")]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _assignmentService.UnassignAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
