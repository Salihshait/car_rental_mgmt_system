using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(await _notificationService.GetForUserAsync(CurrentUserId, cancellationToken));

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.MarkReadAsync(CurrentUserId, id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
