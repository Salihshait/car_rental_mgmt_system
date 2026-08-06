using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IVehicleWarrantyService
{
    Task<IEnumerable<VehicleWarrantyDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleWarrantyDto> CreateAsync(CreateVehicleWarrantyRequest request, CancellationToken cancellationToken = default);
}
