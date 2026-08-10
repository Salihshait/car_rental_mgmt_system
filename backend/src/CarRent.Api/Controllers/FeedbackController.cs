using System.Security.Claims;
using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/feedback")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private const string CrmStaffRoles = "Super Admin,Company Admin,Branch Manager,Customer Support";

    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _feedbackService.CreateAsync(CurrentUserId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(await _feedbackService.GetForCustomerAsync(CurrentUserId, cancellationToken));

    [HttpGet]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] bool? isPublished, CancellationToken cancellationToken) =>
        Ok(await _feedbackService.GetAllAsync(category, isPublished, cancellationToken));

    [HttpPatch("{id:guid}/publish")]
    [Authorize(Roles = CrmStaffRoles)]
    public async Task<IActionResult> SetPublished(Guid id, [FromBody] PublishFeedbackRequest request, CancellationToken cancellationToken) =>
        Ok(await _feedbackService.SetPublishedAsync(id, request, cancellationToken));
}
