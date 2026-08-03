using CarRent.Application.DTOs.Customers;

namespace CarRent.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerSummaryDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
}
