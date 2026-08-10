using System.Text.Json;
using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class DamageDetectionService : IDamageDetectionService
{
    private readonly CarRentDbContext _context;
    private readonly IDamageDetectionProvider _provider;

    public DamageDetectionService(CarRentDbContext context, IDamageDetectionProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<DamageDetectionResultDto> AnalyzeAsync(Guid? vehicleId, Guid? rentalId, string imageReference, byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var analysis = await _provider.AnalyzeAsync(imageBytes, cancellationToken);

        var result = new DamageDetectionResult
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            RentalId = rentalId,
            ImageReference = imageReference,
            DetectedDamagesJson = JsonSerializer.Serialize(analysis.Damages),
            SeverityScore = analysis.SeverityScore
        };

        await _context.DamageDetectionResults.AddAsync(result, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new DamageDetectionResultDto(result.Id, result.VehicleId, result.RentalId, result.ImageReference, analysis.Damages, result.SeverityScore, result.CreatedAt);
    }

    public async Task<IEnumerable<DamageDetectionResultDto>> GetHistoryAsync(Guid? vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _context.DamageDetectionResults.AsNoTracking().AsQueryable();
        if (vehicleId.HasValue)
        {
            query = query.Where(d => d.VehicleId == vehicleId);
        }

        var results = await query.OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);
        return results.Select(r => new DamageDetectionResultDto(
            r.Id, r.VehicleId, r.RentalId, r.ImageReference,
            JsonSerializer.Deserialize<List<DamagedAreaDto>>(r.DetectedDamagesJson) ?? new List<DamagedAreaDto>(),
            r.SeverityScore, r.CreatedAt));
    }
}
