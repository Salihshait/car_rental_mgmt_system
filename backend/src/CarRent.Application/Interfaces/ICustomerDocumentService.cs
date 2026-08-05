using CarRent.Application.DTOs.Customers;

namespace CarRent.Application.Interfaces;

public interface ICustomerDocumentService
{
    Task<IEnumerable<CustomerDocumentDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerDocumentDto> CreateAsync(
        Guid customerId, string documentType, string? documentNumber, DateTime? expiryDate, string? storagePath, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task<CustomerDocumentDto> VerifyAsync(Guid customerId, Guid documentId, string verificationStatus, Guid? actingUserId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid customerId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default);
}
