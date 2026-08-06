using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IMaintenanceExpenseService
{
    Task<IEnumerable<MaintenanceExpenseDto>> GetAllAsync(Guid? vehicleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<MaintenanceExpenseDto> CreateAsync(CreateMaintenanceExpenseRequest request, Guid createdBy, CancellationToken cancellationToken = default);
}
