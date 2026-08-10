using System.Security.Claims;
using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/support-tickets")]
[Authorize]
public class SupportTicketsController : ControllerBase
{
    private const string CrmStaffRoles = "Super Admin,Company Admin,Branch Manager,Customer Support";

    private readonly ISupportTicketService _ticketService;

    public SupportTicketsController(ISupportTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportTicketRequest request, CancellationToken cancellationToken) =>
        Ok(await _ticketService.CreateAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(await _ticketService.GetForCustomerAsync(CurrentUserId, cancellationToken));

    [HttpGet("me/{id:guid}")]
    public async Task<IActionResult> GetMineDetail(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _ticketService.GetDetailAsync(id, cancellationToken);
        return detail.Ticket.CustomerId == CurrentUserId ? Ok(detail) : Forbid();
    }

    [HttpPost("me/{id:guid}/messages")]
    public async Task<IActionResult> ReplyAsCustomer(Guid id, [FromBody] AddTicketMessageRequest request, CancellationToken cancellationToken)
    {
        var detail = await _ticketService.GetDetailAsync(id, cancellationToken);
        if (detail.Ticket.CustomerId != CurrentUserId)
        {
            return Forbid();
        }

        return Ok(await _ticketService.AddMessageAsync(id, CurrentUserId, request with { IsInternalNote = false }, cancellationToken));
    }

    [HttpGet]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? priority, CancellationToken cancellationToken) =>
        Ok(await _ticketService.GetAllAsync(status, priority, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken) =>
        Ok(await _ticketService.GetDetailAsync(id, cancellationToken));

    [HttpPost("{id:guid}/messages")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> ReplyAsStaff(Guid id, [FromBody] AddTicketMessageRequest request, CancellationToken cancellationToken) =>
        Ok(await _ticketService.AddMessageAsync(id, CurrentUserId, request, cancellationToken));

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTicketRequest request, CancellationToken cancellationToken)
    {
        await _ticketService.AssignAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusRequest request, CancellationToken cancellationToken)
    {
        await _ticketService.UpdateStatusAsync(id, request, cancellationToken);
        return NoContent();
    }
}
