using CarRent.Application.DTOs.Insurance;

namespace CarRent.Application.Interfaces;

public interface IInsuranceService
{
    Task<IEnumerable<InsuranceSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InsuranceSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InsuranceSummaryDto> CreateAsync(CreateInsuranceRequest request, CancellationToken cancellationToken = default);
}
