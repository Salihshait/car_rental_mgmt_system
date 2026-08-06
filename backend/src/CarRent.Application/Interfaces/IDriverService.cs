using CarRent.Application.DTOs.Drivers;
using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IDriverService
{
    Task<IEnumerable<DriverDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DriverDto?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DriverDto> CreateAsync(CreateDriverRequest request, CancellationToken cancellationToken = default);
    Task<DriverDto> UpdateAsync(Guid id, UpdateDriverRequest request, CancellationToken cancellationToken = default);
    Task<DriverDto> SelfUpdateAsync(Guid userId, SelfUpdateDriverRequest request, CancellationToken cancellationToken = default);

    Task<DriverPerformanceSummaryDto> GetPerformanceSummaryAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<DriverManagementDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
