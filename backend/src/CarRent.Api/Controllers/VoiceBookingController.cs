using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class SubmitVoiceBookingRequest
{
    public IFormFile? Audio { get; set; }
}

[ApiController]
[Route("api/ai/voice-booking")]
[Authorize]
public class VoiceBookingController : ControllerBase
{
    private readonly IVoiceBookingService _voiceBookingService;

    public VoiceBookingController(IVoiceBookingService voiceBookingService)
    {
        _voiceBookingService = voiceBookingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromForm] SubmitVoiceBookingRequest request, CancellationToken cancellationToken)
    {
        if (request.Audio is null || request.Audio.Length == 0)
        {
            return BadRequest(new { message = "An audio file is required." });
        }

        using var stream = new MemoryStream();
        await request.Audio.CopyToAsync(stream, cancellationToken);

        var result = await _voiceBookingService.SubmitAsync(CurrentUserId, stream.ToArray(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) =>
        Ok(await _voiceBookingService.GetForCustomerAsync(CurrentUserId, cancellationToken));
}
