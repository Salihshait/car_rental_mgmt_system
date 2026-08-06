using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IVehicleMaintenanceService
{
    Task<IEnumerable<VehicleMaintenanceDto>> GetAllAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default);
    Task<VehicleMaintenanceDto> ScheduleAsync(CreateMaintenanceRequest request, Guid createdBy, CancellationToken cancellationToken = default);
    Task<VehicleMaintenanceDto> StartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleMaintenanceDto> CompleteAsync(Guid id, CompleteMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<VehicleMaintenanceDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
