using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface IComplaintService
{
    Task<ComplaintDto> CreateAsync(Guid customerId, CreateComplaintRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status, string? severity, CancellationToken cancellationToken = default);
    Task<ComplaintDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ComplaintDto> ResolveAsync(Guid id, ResolveComplaintRequest request, CancellationToken cancellationToken = default);
}
