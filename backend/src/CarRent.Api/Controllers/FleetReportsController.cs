using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/reports/fleet")]
[Authorize(Roles = AdminRoles)]
public class FleetReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IFleetReportService _reportService;

    public FleetReportsController(IFleetReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetDashboardAsync(from, to, branchId, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportAsync(format, from, to, branchId, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "fleet-report", format, bytes);
    }
}
