using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/reports/bookings")]
[Authorize(Roles = AdminRoles)]
public class BookingsReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IBookingsReportService _reportService;

    public BookingsReportsController(IBookingsReportService reportService)
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
        return ReportFileHelper.ToFileResult(this, "bookings-report", format, bytes);
    }
}
