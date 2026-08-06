using System.Security.Claims;
using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/refunds")]
[Authorize(Roles = AdminRoles)]
public class RefundsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IRefundService _refundService;

    public RefundsController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? bookingId, CancellationToken cancellationToken) =>
        Ok(await _refundService.GetAllAsync(bookingId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRefundRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _refundService.CreateAsync(request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _refundService.ApproveAsync(id, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRefundRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _refundService.RejectAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
