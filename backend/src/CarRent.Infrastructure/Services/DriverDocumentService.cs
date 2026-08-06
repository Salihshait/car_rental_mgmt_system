using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverDocumentService : IDriverDocumentService
{
    private readonly CarRentDbContext _context;

    public DriverDocumentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DriverDocumentDto>> GetByDriverAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _context.DriverDocuments
            .AsNoTracking()
            .Where(d => d.DriverId == driverId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => ToDto(d))
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverDocumentDto> CreateAsync(
        Guid driverId, string documentType, string? documentNumber, DateTime? expiryDate, string? storagePath, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = new DriverDocument
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            ExpiryDate = expiryDate,
            StoragePath = storagePath,
            VerificationStatus = "Pending"
        };

        await _context.DriverDocuments.AddAsync(document, cancellationToken);
        await WriteAuditAsync(driverId, "DocumentUploaded", $"{documentType} document uploaded.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(document);
    }

    public async Task<DriverDocumentDto> VerifyAsync(Guid driverId, Guid documentId, string verificationStatus, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.DriverDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.DriverId == driverId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        document.VerificationStatus = verificationStatus;
        await WriteAuditAsync(driverId, "DocumentVerified", $"{document.DocumentType} document marked {verificationStatus}.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(document);
    }

    public async Task DeleteAsync(Guid driverId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.DriverDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.DriverId == driverId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        _context.DriverDocuments.Remove(document);
        await WriteAuditAsync(driverId, "DocumentDeleted", $"{document.DocumentType} document deleted.", actingUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(Guid driverId, string action, string message, Guid? actingUserId, CancellationToken cancellationToken)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = action,
            EntityType = "Driver",
            EntityId = driverId,
            Payload = $"{{\"message\":\"{message.Replace("\"", "'")}\"}}"
        }, cancellationToken);
    }

    private static DriverDocumentDto ToDto(DriverDocument d) => new()
    {
        Id = d.Id,
        DriverId = d.DriverId,
        DocumentType = d.DocumentType,
        DocumentNumber = d.DocumentNumber,
        ExpiryDate = d.ExpiryDate,
        StoragePath = d.StoragePath,
        VerificationStatus = d.VerificationStatus,
        UploadedAt = d.UploadedAt
    };
}
