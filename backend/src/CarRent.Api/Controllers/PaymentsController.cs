using System.Security.Claims;
using CarRent.Application.DTOs.Payments;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? bookingId, [FromQuery] Guid? invoiceId, CancellationToken cancellationToken) =>
        Ok(await _paymentService.GetAllAsync(bookingId, invoiceId, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _paymentService.InitiateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _paymentService.ConfirmAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("manual")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> RecordManual([FromBody] RecordManualPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _paymentService.RecordManualPaymentAsync(request, CurrentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
