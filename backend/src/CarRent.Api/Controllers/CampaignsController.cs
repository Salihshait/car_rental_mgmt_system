using System.Security.Claims;
using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/crm/campaigns")]
[Authorize(Roles = CrmStaffRoles)]
public class CampaignsController : ControllerBase
{
    private const string CrmStaffRoles = "Super Admin,Company Admin,Branch Manager,Customer Support";

    private readonly ICampaignService _campaignService;
    private readonly ICampaignExecutionService _campaignExecutionService;

    public CampaignsController(ICampaignService campaignService, ICampaignExecutionService campaignExecutionService)
    {
        _campaignService = campaignService;
        _campaignExecutionService = campaignExecutionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await _campaignService.GetAllAsync(status, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _campaignService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _campaignService.CreateAsync(CurrentUserId, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/schedule")]
    public async Task<IActionResult> Schedule(Guid id, [FromBody] ScheduleCampaignRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _campaignService.ScheduleAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/send-now")]
    public async Task<IActionResult> SendNow(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _campaignExecutionService.SendNowAsync(id, cancellationToken);
            return Ok(await _campaignService.GetByIdAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _campaignService.CancelAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _campaignService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/logs")]
    public async Task<IActionResult> GetLogs(Guid id, CancellationToken cancellationToken) =>
        Ok(await _campaignService.GetLogsAsync(id, cancellationToken));
}
