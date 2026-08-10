using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/cashbook")]
[Authorize(Roles = AdminRoles)]
public class CashbookController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ICashbookService _cashbookService;

    public CashbookController(ICashbookService cashbookService)
    {
        _cashbookService = cashbookService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _cashbookService.GetAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _cashbookService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "cashbook", format, bytes);
    }
}
