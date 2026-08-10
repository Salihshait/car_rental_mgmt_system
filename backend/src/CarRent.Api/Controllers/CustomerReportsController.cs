using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/reports/customers")]
[Authorize(Roles = AdminRoles)]
public class CustomerReportsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ICustomerReportService _reportService;

    public CustomerReportsController(ICustomerReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _reportService.GetDashboardAsync(from, to, cancellationToken));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string format, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var bytes = await _reportService.ExportAsync(format, from, to, cancellationToken);
        return ReportFileHelper.ToFileResult(this, "customer-report", format, bytes);
    }
}
