using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/balance-sheet")]
[Authorize(Roles = AdminRoles)]
public class BalanceSheetController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IBalanceSheetService _balanceSheetService;

    public BalanceSheetController(IBalanceSheetService balanceSheetService)
    {
        _balanceSheetService = balanceSheetService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? asOfDate, CancellationToken cancellationToken) =>
        Ok(await _balanceSheetService.GetAsync(asOfDate, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? asOfDate, CancellationToken cancellationToken)
    {
        var bytes = await _balanceSheetService.ExportAsync(format, asOfDate, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "balance-sheet", format, bytes);
    }
}
