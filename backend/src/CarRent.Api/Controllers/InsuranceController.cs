using CarRent.Application.DTOs.Insurance;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsuranceController : ControllerBase
{
    private readonly IInsuranceService _insuranceService;

    public InsuranceController(IInsuranceService insuranceService)
    {
        _insuranceService = insuranceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var insurances = await _insuranceService.GetAllAsync(cancellationToken);
        return Ok(insurances);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var insurance = await _insuranceService.GetByIdAsync(id, cancellationToken);
        return insurance is null ? NotFound() : Ok(insurance);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _insuranceService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
