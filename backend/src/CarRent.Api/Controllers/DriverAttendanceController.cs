using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/driver-attendance")]
[Authorize(Roles = AdminRoles)]
public class DriverAttendanceController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDriverAttendanceService _attendanceService;

    public DriverAttendanceController(IDriverAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("{driverId:guid}")]
    public async Task<IActionResult> GetAttendance(Guid driverId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _attendanceService.GetAttendanceAsync(driverId, from, to, cancellationToken));

    [HttpPost("{driverId:guid}/mark")]
    public async Task<IActionResult> Mark(Guid driverId, [FromBody] MarkAttendanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _attendanceService.MarkAsync(driverId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
