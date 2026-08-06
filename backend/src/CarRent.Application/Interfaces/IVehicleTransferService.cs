using CarRent.Application.DTOs.Fleet;

namespace CarRent.Application.Interfaces;

public interface IVehicleTransferService
{
    Task<IEnumerable<VehicleTransferDto>> GetAllAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default);
    Task<VehicleTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedBy, CancellationToken cancellationToken = default);
    Task<VehicleTransferDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleTransferDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
}
