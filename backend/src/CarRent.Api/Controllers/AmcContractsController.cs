using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/amc-contracts")]
[Authorize(Roles = AdminRoles)]
public class AmcContractsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IAmcContractService _amcContractService;

    public AmcContractsController(IAmcContractService amcContractService)
    {
        _amcContractService = amcContractService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? vehicleId, CancellationToken cancellationToken) =>
        Ok(await _amcContractService.GetAllAsync(vehicleId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAmcContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _amcContractService.CreateAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
