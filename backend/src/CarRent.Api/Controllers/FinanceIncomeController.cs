using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/income")]
[Authorize(Roles = AdminRoles)]
public class FinanceIncomeController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IIncomeService _incomeService;

    public FinanceIncomeController(IIncomeService incomeService)
    {
        _incomeService = incomeService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _incomeService.GetSummaryAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _incomeService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "income-report", format, bytes);
    }
}
