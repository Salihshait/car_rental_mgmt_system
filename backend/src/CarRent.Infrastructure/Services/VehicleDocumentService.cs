using CarRent.Application.DTOs.Vehicles;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class VehicleDocumentService : IVehicleDocumentService
{
    private readonly CarRentDbContext _context;

    public VehicleDocumentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleDocumentDto>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.VehicleDocuments
            .AsNoTracking()
            .Where(d => d.VehicleId == vehicleId)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new VehicleDocumentDto
            {
                Id = d.Id,
                VehicleId = d.VehicleId,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                IssuedBy = d.IssuedBy,
                ExpiryDate = d.ExpiryDate,
                StoragePath = d.StoragePath,
                UploadedAt = d.UploadedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleDocumentDto> CreateAsync(
        Guid vehicleId,
        string documentType,
        string? documentNumber,
        string? issuedBy,
        DateTime? expiryDate,
        string? storagePath,
        Guid? actingUserId,
        CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == vehicleId, cancellationToken);
        if (!vehicleExists)
        {
            throw new InvalidOperationException("Vehicle not found.");
        }

        var document = new VehicleDocument
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            IssuedBy = issuedBy,
            ExpiryDate = expiryDate,
            StoragePath = storagePath
        };

        await _context.VehicleDocuments.AddAsync(document, cancellationToken);
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = "DocumentUploaded",
            EntityType = "Vehicle",
            EntityId = vehicleId,
            Payload = $"{{\"documentType\":\"{documentType}\"}}"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new VehicleDocumentDto
        {
            Id = document.Id,
            VehicleId = document.VehicleId,
            DocumentType = document.DocumentType,
            DocumentNumber = document.DocumentNumber,
            IssuedBy = document.IssuedBy,
            ExpiryDate = document.ExpiryDate,
            StoragePath = document.StoragePath,
            UploadedAt = document.UploadedAt
        };
    }

    public async Task DeleteAsync(Guid vehicleId, Guid documentId, Guid? actingUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.VehicleDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.VehicleId == vehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Document not found.");

        _context.VehicleDocuments.Remove(document);
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = "DocumentDeleted",
            EntityType = "Vehicle",
            EntityId = vehicleId,
            Payload = $"{{\"documentType\":\"{document.DocumentType}\"}}"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
