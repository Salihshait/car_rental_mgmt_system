using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IAmcContractService
{
    Task<IEnumerable<AmcContractDto>> GetAllAsync(Guid? vehicleId, CancellationToken cancellationToken = default);
    Task<AmcContractDto> CreateAsync(CreateAmcContractRequest request, CancellationToken cancellationToken = default);
}
