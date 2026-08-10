using System.Security.Claims;
using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private const string CrmStaffRoles = "Super Admin,Company Admin,Branch Manager,Customer Support";

    private readonly IComplaintService _complaintService;

    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest request, CancellationToken cancellationToken) =>
        Ok(await _complaintService.CreateAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(await _complaintService.GetForCustomerAsync(CurrentUserId, cancellationToken));

    [HttpGet]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? severity, CancellationToken cancellationToken) =>
        Ok(await _complaintService.GetAllAsync(status, severity, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _complaintService.GetByIdAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveComplaintRequest request, CancellationToken cancellationToken) =>
        Ok(await _complaintService.ResolveAsync(id, request, cancellationToken));
}
