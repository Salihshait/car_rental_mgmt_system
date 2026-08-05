using CarRent.Application.DTOs.Departments;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly CarRentDbContext _context;

    public DepartmentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DepartmentSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query().ToListAsync(cancellationToken);
    }

    public async Task<DepartmentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DepartmentSummaryDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var nameInUse = await _context.Departments.AnyAsync(d => d.Name == request.Name, cancellationToken);
        if (nameInUse)
        {
            throw new InvalidOperationException("A department with this name already exists.");
        }

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BranchId = request.BranchId
        };

        await _context.Departments.AddAsync(department, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(department.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to load created department.");
    }

    public async Task<DepartmentSummaryDto> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Department not found.");

        department.Name = request.Name;
        department.Description = request.Description;
        department.BranchId = request.BranchId;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException("Failed to load updated department.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Department not found.");

        var inUse = await _context.Users.AnyAsync(u => u.DepartmentId == id, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("This department has users assigned and cannot be deleted.");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<DepartmentSummaryDto> Query()
    {
        return _context.Departments
            .AsNoTracking()
            .Include(d => d.Branch)
            .Select(d => new DepartmentSummaryDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                BranchId = d.BranchId,
                BranchName = d.Branch != null ? d.Branch.Name : null
            });
    }
}
