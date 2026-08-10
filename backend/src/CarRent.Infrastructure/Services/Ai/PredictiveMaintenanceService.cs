using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class PredictiveMaintenanceService : IPredictiveMaintenanceService
{
    private const int ServiceIntervalDays = 90;
    private const int FirstServiceMileageThreshold = 8000;

    private readonly CarRentDbContext _context;

    public PredictiveMaintenanceService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task GeneratePredictionsAsync(CancellationToken cancellationToken = default)
    {
        var vehicles = await _context.Vehicles.AsNoTracking().ToListAsync(cancellationToken);
        var completedMaintenance = await _context.VehicleMaintenances.AsNoTracking().Where(m => m.Status == "Completed").ToListAsync(cancellationToken);

        foreach (var vehicle in vehicles)
        {
            var lastService = completedMaintenance
                .Where(m => m.VehicleId == vehicle.Id)
                .OrderByDescending(m => m.CompletedAt ?? m.ScheduledOn)
                .FirstOrDefault();

            if (lastService is not null)
            {
                var referenceDate = lastService.CompletedAt ?? lastService.ScheduledOn;
                var daysSinceService = (DateTime.UtcNow - referenceDate).TotalDays;
                if (daysSinceService >= ServiceIntervalDays)
                {
                    var dueDate = referenceDate.AddDays(ServiceIntervalDays);
                    var confidence = Math.Min(95m, 50m + (decimal)(daysSinceService - ServiceIntervalDays));
                    await UpsertPredictionAsync(vehicle.Id, "Scheduled Service Due", dueDate, confidence,
                        $"{Math.Round(daysSinceService)} days since last completed service (interval: {ServiceIntervalDays} days).", cancellationToken);
                }
            }
            else if (vehicle.CurrentOdometerReading.HasValue && vehicle.CurrentOdometerReading.Value >= FirstServiceMileageThreshold)
            {
                var confidence = Math.Min(90m, 40m + (vehicle.CurrentOdometerReading.Value - FirstServiceMileageThreshold) / 200m);
                await UpsertPredictionAsync(vehicle.Id, "First Service Overdue", DateTime.UtcNow, confidence,
                    $"No completed maintenance on record and odometer is at {vehicle.CurrentOdometerReading} km (threshold: {FirstServiceMileageThreshold} km).", cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenancePredictionDto>> GetPredictionsAsync(Guid? vehicleId, string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenancePredictions.AsNoTracking().AsQueryable();
        if (vehicleId.HasValue) query = query.Where(p => p.VehicleId == vehicleId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);

        var predictions = await query.OrderBy(p => p.PredictedDueDate).ToListAsync(cancellationToken);
        var vehicleIds = predictions.Select(p => p.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return predictions.Select(p => MapDto(p, vehicles.FirstOrDefault(v => v.Id == p.VehicleId)));
    }

    public async Task<MaintenancePredictionDto> UpdateStatusAsync(Guid id, UpdatePredictionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var prediction = await _context.MaintenancePredictions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Prediction not found.");

        prediction.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == prediction.VehicleId, cancellationToken);
        return MapDto(prediction, vehicle);
    }

    private async Task UpsertPredictionAsync(Guid vehicleId, string issue, DateTime dueDate, decimal confidence, string basis, CancellationToken cancellationToken)
    {
        var existing = await _context.MaintenancePredictions
            .FirstOrDefaultAsync(p => p.VehicleId == vehicleId && p.PredictedIssue == issue && p.Status == "Open", cancellationToken);

        if (existing is not null)
        {
            existing.PredictedDueDate = dueDate;
            existing.ConfidenceScore = confidence;
            existing.BasisSummary = basis;
        }
        else
        {
            await _context.MaintenancePredictions.AddAsync(new MaintenancePrediction
            {
                Id = Guid.NewGuid(),
                VehicleId = vehicleId,
                PredictedIssue = issue,
                PredictedDueDate = dueDate,
                ConfidenceScore = confidence,
                BasisSummary = basis,
                Status = "Open"
            }, cancellationToken);
        }
    }

    private static MaintenancePredictionDto MapDto(MaintenancePrediction prediction, Vehicle? vehicle) => new(
        prediction.Id, prediction.VehicleId, vehicle?.RegistrationNumber, prediction.PredictedIssue, prediction.PredictedDueDate,
        prediction.ConfidenceScore, prediction.BasisSummary, prediction.Status, prediction.CreatedAt);
}
