using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/ledger")]
[Authorize(Roles = AdminRoles)]
public class LedgerController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ILedgerService _ledgerService;

    public LedgerController(ILedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _ledgerService.GetAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _ledgerService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "ledger", format, bytes);
    }
}
