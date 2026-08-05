using CarRent.Application.DTOs.Customers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class CustomerDocumentService : ICustomerDocumentService
{
    private readonly CarRentDbContext _context;

    public CustomerDocumentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDocumentDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerDocuments
            .AsNoTracking()
            .Where(d => d.CustomerId == customerId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => ToDto(d))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDocumentDto> CreateAsync(
        Guid customerId, string documentType, string? documentNumber, DateTime? expiryDate, string? storagePath, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = new CustomerDocument
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            ExpiryDate = expiryDate,
            StoragePath = storagePath,
            VerificationStatus = "Pending"
        };

        await _context.CustomerDocuments.AddAsync(document, cancellationToken);
        await WriteAuditAsync(customerId, "DocumentUploaded", $"{documentType} document uploaded.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(document);
    }

    public async Task<CustomerDocumentDto> VerifyAsync(Guid customerId, Guid documentId, string verificationStatus, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.CustomerDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        document.VerificationStatus = verificationStatus;
        await WriteAuditAsync(customerId, "DocumentVerified", $"{document.DocumentType} document marked {verificationStatus}.", actingUserId, cancellationToken);
        await _context.Notifications.AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = customerId,
            NotificationType = "Document",
            Message = $"Your {document.DocumentType} document was marked {verificationStatus}."
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(document);
    }

    public async Task DeleteAsync(Guid customerId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.CustomerDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.CustomerId == customerId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        _context.CustomerDocuments.Remove(document);
        await WriteAuditAsync(customerId, "DocumentDeleted", $"{document.DocumentType} document deleted.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(Guid customerId, string action, string message, Guid? actingUserId, CancellationToken cancellationToken)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = action,
            EntityType = "Customer",
            EntityId = customerId,
            Payload = $"{{\"message\":\"{message.Replace("\"", "'")}\"}}"
        }, cancellationToken);
    }

    private static CustomerDocumentDto ToDto(CustomerDocument d) => new()
    {
        Id = d.Id,
        CustomerId = d.CustomerId,
        DocumentType = d.DocumentType,
        DocumentNumber = d.DocumentNumber,
        ExpiryDate = d.ExpiryDate,
        StoragePath = d.StoragePath,
        VerificationStatus = d.VerificationStatus,
        UploadedAt = d.UploadedAt
    };
}
