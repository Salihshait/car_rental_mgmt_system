using System.Security.Claims;
using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/fraud")]
[Authorize(Roles = AdminRoles)]
public class FraudDetectionController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IFraudDetectionService _fraudDetectionService;

    public FraudDetectionController(IFraudDetectionService fraudDetectionService)
    {
        _fraudDetectionService = fraudDetectionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("evaluate/{bookingId:guid}")]
    public async Task<IActionResult> Evaluate(Guid bookingId, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _fraudDetectionService.EvaluateBookingAsync(bookingId, cancellationToken);
            return Ok(alert);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _fraudDetectionService.GetAlertsAsync(status, cancellationToken));

    [HttpPatch("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewFraudAlertRequest request, CancellationToken cancellationToken) =>
        Ok(await _fraudDetectionService.ReviewAlertAsync(id, CurrentUserId, request, cancellationToken));
}
