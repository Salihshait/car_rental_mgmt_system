using CarRent.Application.DTOs.Vehicles;

namespace CarRent.Application.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<VehicleDto>> GetAllAsync(VehicleFilter filter, CancellationToken cancellationToken = default);
    Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleDto> CreateAsync(SaveVehicleRequest request, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<VehicleDto> UpdateAsync(Guid id, SaveVehicleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleDto> UpdateStatusAsync(Guid id, string status, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleTimelineEntryDto>> GetTimelineAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleImportResultDto> ImportAsync(Stream csvStream, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(VehicleFilter filter, CancellationToken cancellationToken = default);
    Task<FleetDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
