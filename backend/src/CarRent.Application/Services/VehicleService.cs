using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;

namespace CarRent.Application.Services;

public interface IVehicleServiceApplication
{
    Task<IEnumerable<VehicleListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VehicleListDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleListDto> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
