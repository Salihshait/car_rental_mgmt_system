using System.Text.Json;
using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class DocumentOcrService : IDocumentOcrService
{
    private readonly CarRentDbContext _context;
    private readonly IOcrProvider _provider;

    public DocumentOcrService(CarRentDbContext context, IOcrProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<OcrResultDto> ExtractAsync(string documentType, byte[] imageBytes, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var extraction = await _provider.ExtractAsync(documentType, imageBytes, cancellationToken);

        var result = new DocumentOcrResult
        {
            Id = Guid.NewGuid(),
            DocumentType = documentType,
            ExtractedFieldsJson = JsonSerializer.Serialize(extraction.Fields),
            ConfidenceScore = extraction.ConfidenceScore,
            CreatedByUserId = createdByUserId
        };

        await _context.DocumentOcrResults.AddAsync(result, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new OcrResultDto(result.Id, result.DocumentType, extraction.Fields, result.ConfidenceScore, result.CreatedAt);
    }

    public async Task<IEnumerable<OcrResultDto>> GetHistoryAsync(string? documentType, CancellationToken cancellationToken = default)
    {
        var query = _context.DocumentOcrResults.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(documentType))
        {
            query = query.Where(d => d.DocumentType == documentType);
        }

        var results = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);
        return results.Select(r => new OcrResultDto(
            r.Id, r.DocumentType,
            JsonSerializer.Deserialize<Dictionary<string, string>>(r.ExtractedFieldsJson) ?? new Dictionary<string, string>(),
            r.ConfidenceScore, r.CreatedAt));
    }
}
