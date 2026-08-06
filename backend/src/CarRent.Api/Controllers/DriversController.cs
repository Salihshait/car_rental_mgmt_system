using System.Security.Claims;
using CarRent.Application.DTOs.Fleet;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public class DriversController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IDriverService _driverService;
    private readonly IDriverAssignmentService _assignmentService;
    private readonly IFleetTrackingService _trackingService;
    private readonly IDriverAttendanceService _attendanceService;
    private readonly IDriverSalaryService _salaryService;
    private readonly IDriverRatingService _ratingService;

    public DriversController(
        IDriverService driverService,
        IDriverAssignmentService assignmentService,
        IFleetTrackingService trackingService,
        IDriverAttendanceService attendanceService,
        IDriverSalaryService salaryService,
        IDriverRatingService ratingService)
    {
        _driverService = driverService;
        _assignmentService = assignmentService;
        _trackingService = trackingService;
        _attendanceService = attendanceService;
        _salaryService = salaryService;
        _ratingService = ratingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _driverService.GetAllAsync(cancellationToken));

    [HttpGet("dashboard")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await _driverService.GetDashboardAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await _driverService.GetByIdAsync(id, cancellationToken);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpGet("{id:guid}/performance")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetPerformance(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _driverService.GetPerformanceSummaryAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _driverService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDriverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _driverService.UpdateAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Self-service ("Driver App Dashboard")

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var driver = await _driverService.GetByUserIdAsync(CurrentUserId, cancellationToken);
        return driver is null ? NotFound() : Ok(driver);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] SelfUpdateDriverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _driverService.SelfUpdateAsync(CurrentUserId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me/assignment")]
    public async Task<IActionResult> GetMyAssignment(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        var history = await _assignmentService.GetHistoryAsync(null, driver.Id, cancellationToken);
        var active = history.FirstOrDefault(a => a.UnassignedAt is null);
        return Ok(active);
    }

    [HttpGet("me/trips")]
    public async Task<IActionResult> GetMyTrips(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        return Ok(await _trackingService.GetTripsAsync(new TripFilter { DriverId = driver.Id }, cancellationToken));
    }

    [HttpGet("me/attendance")]
    public async Task<IActionResult> GetMyAttendance([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        return Ok(await _attendanceService.GetAttendanceAsync(driver.Id, from, to, cancellationToken));
    }

    [HttpPost("me/attendance/check-in")]
    public async Task<IActionResult> CheckIn(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        try
        {
            return Ok(await _attendanceService.CheckInAsync(driver.Id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("me/attendance/check-out")]
    public async Task<IActionResult> CheckOut(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        try
        {
            return Ok(await _attendanceService.CheckOutAsync(driver.Id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me/salary")]
    public async Task<IActionResult> GetMySalary(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        return Ok(await _salaryService.GetAllAsync(driver.Id, cancellationToken));
    }

    [HttpGet("me/ratings")]
    public async Task<IActionResult> GetMyRatings(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        return Ok(await _ratingService.GetAllAsync(driver.Id, cancellationToken));
    }

    [HttpGet("me/performance")]
    public async Task<IActionResult> GetMyPerformance(CancellationToken cancellationToken)
    {
        var driver = await RequireSelfDriverAsync(cancellationToken);
        if (driver is null) return NotFound();

        return Ok(await _driverService.GetPerformanceSummaryAsync(driver.Id, cancellationToken));
    }

    private Task<DriverDto?> RequireSelfDriverAsync(CancellationToken cancellationToken) =>
        _driverService.GetByUserIdAsync(CurrentUserId, cancellationToken);
}
