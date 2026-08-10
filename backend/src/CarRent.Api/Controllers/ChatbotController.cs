using System.Security.Claims;
using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/chatbot")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;

    public ChatbotController(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("sessions")]
    public async Task<IActionResult> StartSession(CancellationToken cancellationToken) =>
        Ok(await _chatbotService.StartSessionAsync(CurrentUserId, cancellationToken));

    [HttpPost("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _chatbotService.SendMessageAsync(sessionId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<IActionResult> GetHistory(Guid sessionId, CancellationToken cancellationToken) =>
        Ok(await _chatbotService.GetHistoryAsync(sessionId, cancellationToken));
}
