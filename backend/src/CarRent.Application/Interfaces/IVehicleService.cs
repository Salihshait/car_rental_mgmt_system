using CarRent.Application.DTOs.Vehicles;

namespace CarRent.Application.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<VehicleListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VehicleListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleListDto> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
