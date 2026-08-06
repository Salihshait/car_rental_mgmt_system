using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IVehicleInspectionService
{
    Task<IEnumerable<VehicleInspectionDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleInspectionDto> CreateAsync(CreateVehicleInspectionRequest request, CancellationToken cancellationToken = default);
}
