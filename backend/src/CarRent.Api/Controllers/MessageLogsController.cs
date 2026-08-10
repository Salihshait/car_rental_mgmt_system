using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/crm/message-logs")]
[Authorize(Roles = CrmStaffRoles)]
public class MessageLogsController : ControllerBase
{
    private const string CrmStaffRoles = "Super Admin,Company Admin,Branch Manager,Customer Support";

    private readonly IMessageLogService _messageLogService;

    public MessageLogsController(IMessageLogService messageLogService)
    {
        _messageLogService = messageLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? channel,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken) =>
        Ok(await _messageLogService.GetAllAsync(channel, status, from, to, cancellationToken));

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendAdHocMessageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _messageLogService.SendAdHocAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
