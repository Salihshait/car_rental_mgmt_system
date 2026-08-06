using System.Security.Claims;
using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private bool IsAdmin => User.IsInRole("Super Admin") || User.IsInRole("Company Admin") || User.IsInRole("Branch Manager");

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? vehicleId,
        [FromQuery] Guid? branchId,
        [FromQuery] string? status,
        [FromQuery] string? bookingType,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var filter = new BookingFilter
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            BranchId = branchId,
            Status = status,
            BookingType = bookingType,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        return Ok(await _bookingService.GetAllAsync(filter, CurrentUserId, IsAdmin, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.GetTimelineAsync(id, CurrentUserId, IsAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("quote")]
    public async Task<IActionResult> Quote([FromBody] BookingQuoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.QuoteAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("online")]
    public async Task<IActionResult> CreateOnline([FromBody] CreateOnlineBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _bookingService.CreateOnlineAsync(CurrentUserId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("walk-in")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> CreateWalkIn([FromBody] CreateWalkInBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _bookingService.CreateWalkInAsync(CurrentUserId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.UpdateAsync(id, request, CurrentUserId, IsAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.CancelAsync(id, request, CurrentUserId, IsAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.ApproveAsync(id, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.RejectAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/extend")]
    public async Task<IActionResult> Extend(Guid id, [FromBody] ExtendBookingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _bookingService.ExtendAsync(id, request, CurrentUserId, IsAdmin, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("reports/summary")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetReportSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetReportSummaryAsync(from, to, branchId, cancellationToken));
}
