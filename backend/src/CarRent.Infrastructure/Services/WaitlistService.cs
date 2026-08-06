using CarRent.Application.DTOs.Waitlist;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class WaitlistService : IWaitlistService
{
    private readonly CarRentDbContext _context;
    private readonly IBookingNotificationService _notificationService;

    public WaitlistService(CarRentDbContext context, IBookingNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<WaitlistDto>> GetAllAsync(WaitlistFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.WaitlistEntries.AsNoTracking().AsQueryable();

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(w => w.CustomerId == filter.CustomerId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(w => w.Status == filter.Status);
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(w => w.BranchId == filter.BranchId);
        }

        if (filter.VehicleCategoryId.HasValue)
        {
            query = query.Where(w => w.VehicleCategoryId == filter.VehicleCategoryId);
        }

        var entries = await query.OrderByDescending(w => w.CreatedAt).ToListAsync(cancellationToken);
        return await MapAsync(entries, cancellationToken);
    }

    public async Task<WaitlistDto> CreateAsync(Guid customerId, CreateWaitlistRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DesiredEndDate <= request.DesiredStartDate)
        {
            throw new InvalidOperationException("Desired end date must be after the desired start date.");
        }

        if (request.VehicleId is null && request.VehicleCategoryId is null)
        {
            throw new InvalidOperationException("Either a specific vehicle or a vehicle category must be provided.");
        }

        var entry = new WaitlistEntry
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = request.VehicleId,
            VehicleCategoryId = request.VehicleCategoryId,
            BranchId = request.BranchId,
            DesiredStartDate = request.DesiredStartDate,
            DesiredEndDate = request.DesiredEndDate,
            Status = "Waiting"
        };

        await _context.WaitlistEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<WaitlistEntry> { entry }, cancellationToken);
        return mapped.First();
    }

    public async Task CancelAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var entry = await _context.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Waitlist entry not found.");

        if (!isAdmin && entry.CustomerId != callerUserId)
        {
            throw new InvalidOperationException("You do not have access to this waitlist entry.");
        }

        entry.Status = "Cancelled";
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task NotifyMatchingEntriesAsync(Guid vehicleId, DateTime freedStart, DateTime freedEnd, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.VehicleModel)
            .FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken);

        if (vehicle is null)
        {
            return;
        }

        var categoryId = vehicle.VehicleModel?.CategoryId;

        var matches = await _context.WaitlistEntries
            .Where(w => w.Status == "Waiting"
                && freedStart < w.DesiredEndDate && freedEnd > w.DesiredStartDate
                && (w.VehicleId == vehicleId || (categoryId != null && w.VehicleCategoryId == categoryId)))
            .ToListAsync(cancellationToken);

        foreach (var match in matches)
        {
            match.Status = "Notified";
            match.NotifiedAt = DateTime.UtcNow;
            await _notificationService.NotifyWaitlistSlotAvailableAsync(match, cancellationToken);
        }

        if (matches.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<List<WaitlistDto>> MapAsync(List<WaitlistEntry> entries, CancellationToken cancellationToken)
    {
        var customerIds = entries.Select(e => e.CustomerId).Distinct().ToList();
        var vehicleIds = entries.Where(e => e.VehicleId.HasValue).Select(e => e.VehicleId!.Value).Distinct().ToList();
        var categoryIds = entries.Where(e => e.VehicleCategoryId.HasValue).Select(e => e.VehicleCategoryId!.Value).Distinct().ToList();

        var customers = await _context.Users.AsNoTracking().Where(u => customerIds.Contains(u.Id)).ToListAsync(cancellationToken);
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);
        var categories = await _context.VehicleCategories.AsNoTracking().Where(c => categoryIds.Contains(c.Id)).ToListAsync(cancellationToken);

        return entries.Select(e =>
        {
            var customer = customers.FirstOrDefault(u => u.Id == e.CustomerId);
            var vehicle = e.VehicleId.HasValue ? vehicles.FirstOrDefault(v => v.Id == e.VehicleId) : null;
            var category = e.VehicleCategoryId.HasValue ? categories.FirstOrDefault(c => c.Id == e.VehicleCategoryId) : null;

            return new WaitlistDto
            {
                Id = e.Id,
                CustomerId = e.CustomerId,
                CustomerName = customer is null ? null : $"{customer.FirstName} {customer.LastName}",
                VehicleId = e.VehicleId,
                VehicleRegistrationNumber = vehicle?.RegistrationNumber,
                VehicleCategoryId = e.VehicleCategoryId,
                VehicleCategoryName = category?.Name,
                BranchId = e.BranchId,
                DesiredStartDate = e.DesiredStartDate,
                DesiredEndDate = e.DesiredEndDate,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                NotifiedAt = e.NotifiedAt
            };
        }).ToList();
    }
}
