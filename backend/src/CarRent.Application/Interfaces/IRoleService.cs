using CarRent.Application.DTOs.Roles;

namespace CarRent.Application.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleSummaryDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleSummaryDto> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
