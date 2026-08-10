using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/profit-loss")]
[Authorize(Roles = AdminRoles)]
public class ProfitLossController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IProfitLossService _profitLossService;

    public ProfitLossController(IProfitLossService profitLossService)
    {
        _profitLossService = profitLossService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _profitLossService.GetAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _profitLossService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "profit-loss", format, bytes);
    }
}
