using CarRent.Application.DTOs.Departments;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin";

    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetByIdAsync(id, cancellationToken);
        return department is null ? NotFound() : Ok(department);
    }

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _departmentService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _departmentService.UpdateAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _departmentService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
