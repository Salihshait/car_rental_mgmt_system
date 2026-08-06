using CarRent.Application.DTOs.Maintenance;

namespace CarRent.Application.Interfaces;

public interface IVendorService
{
    Task<IEnumerable<VendorDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VendorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VendorDto> CreateAsync(SaveVendorRequest request, CancellationToken cancellationToken = default);
    Task<VendorDto> UpdateAsync(Guid id, SaveVendorRequest request, CancellationToken cancellationToken = default);
}
