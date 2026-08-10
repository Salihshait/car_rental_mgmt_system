using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/finance/gst")]
[Authorize(Roles = AdminRoles)]
public class GstReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IGstReportService _gstReportService;

    public GstReportsController(IGstReportService gstReportService)
    {
        _gstReportService = gstReportService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken) =>
        Ok(await _gstReportService.GetSummaryAsync(from, to, branchId, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var bytes = await _gstReportService.ExportAsync(format, from, to, branchId, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "gst-report", format, bytes);
    }
}
