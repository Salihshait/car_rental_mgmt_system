using CarRent.Application.DTOs.Settings;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";
    private const string TaxRateSettingKey = "TaxRatePercent";
    private const string BillingCategory = "Billing";
    private const decimal DefaultTaxRatePercent = 8m;

    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("tax-rate")]
    public async Task<IActionResult> GetTaxRate(CancellationToken cancellationToken)
    {
        var raw = await _settingsService.GetAsync(TaxRateSettingKey, cancellationToken);
        var rate = decimal.TryParse(raw, out var parsed) ? parsed : DefaultTaxRatePercent;
        return Ok(new TaxSettingDto { TaxRatePercent = rate });
    }

    [HttpPut("tax-rate")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> UpdateTaxRate([FromBody] UpdateTaxSettingRequest request, CancellationToken cancellationToken)
    {
        if (request.TaxRatePercent < 0 || request.TaxRatePercent > 100)
        {
            return BadRequest(new { message = "Tax rate must be between 0 and 100." });
        }

        await _settingsService.SetAsync(TaxRateSettingKey, request.TaxRatePercent.ToString(System.Globalization.CultureInfo.InvariantCulture), BillingCategory, cancellationToken);
        return Ok(new TaxSettingDto { TaxRatePercent = request.TaxRatePercent });
    }
}
