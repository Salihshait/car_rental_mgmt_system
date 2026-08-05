using CarRent.Application.DTOs.Departments;

namespace CarRent.Application.Interfaces;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DepartmentSummaryDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<DepartmentSummaryDto> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
