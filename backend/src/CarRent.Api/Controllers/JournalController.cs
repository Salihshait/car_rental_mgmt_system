using System.Security.Claims;
using CarRent.Application.DTOs.Finance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/journal")]
[Authorize(Roles = AdminRoles)]
public class JournalController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IJournalService _journalService;

    public JournalController(IJournalService journalService)
    {
        _journalService = journalService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? entryType, CancellationToken cancellationToken) =>
        Ok(await _journalService.GetAllAsync(from, to, entryType, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryRequest request, CancellationToken cancellationToken) =>
        Ok(await _journalService.CreateAsync(CurrentUserId, request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateJournalEntryRequest request, CancellationToken cancellationToken) =>
        Ok(await _journalService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _journalService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
