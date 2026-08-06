using CarRent.Application.DTOs.Maintenance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class MaintenanceExpenseService : IMaintenanceExpenseService
{
    private readonly CarRentDbContext _context;

    public MaintenanceExpenseService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MaintenanceExpenseDto>> GetAllAsync(Guid? vehicleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenanceExpenses.AsNoTracking().AsQueryable();

        if (vehicleId.HasValue)
        {
            query = query.Where(e => e.VehicleId == vehicleId);
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= from);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= to);
        }

        var expenses = await query.OrderByDescending(e => e.ExpenseDate).ToListAsync(cancellationToken);
        return await MapAsync(expenses, cancellationToken);
    }

    public async Task<MaintenanceExpenseDto> CreateAsync(CreateMaintenanceExpenseRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        if (request.VehicleId.HasValue && !await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId, cancellationToken))
        {
            throw new InvalidOperationException("Vehicle not found.");
        }

        if (request.MaintenanceId.HasValue && !await _context.VehicleMaintenances.AnyAsync(m => m.Id == request.MaintenanceId, cancellationToken))
        {
            throw new InvalidOperationException("Maintenance record not found.");
        }

        var expense = new MaintenanceExpense
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            MaintenanceId = request.MaintenanceId,
            VendorId = request.VendorId,
            Category = request.Category,
            Amount = request.Amount,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate ?? DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _context.MaintenanceExpenses.AddAsync(expense, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var mapped = await MapAsync(new List<MaintenanceExpense> { expense }, cancellationToken);
        return mapped.First();
    }

    private async Task<List<MaintenanceExpenseDto>> MapAsync(List<MaintenanceExpense> expenses, CancellationToken cancellationToken)
    {
        var vehicleIds = expenses.Where(e => e.VehicleId.HasValue).Select(e => e.VehicleId!.Value).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var vendorIds = expenses.Where(e => e.VendorId.HasValue).Select(e => e.VendorId!.Value).Distinct().ToList();
        var vendors = await _context.Vendors.AsNoTracking().Where(v => vendorIds.Contains(v.Id)).ToListAsync(cancellationToken);

        return expenses.Select(e => new MaintenanceExpenseDto
        {
            Id = e.Id,
            VehicleId = e.VehicleId,
            VehicleRegistrationNumber = e.VehicleId.HasValue ? vehicles.FirstOrDefault(v => v.Id == e.VehicleId)?.RegistrationNumber : null,
            MaintenanceId = e.MaintenanceId,
            VendorId = e.VendorId,
            VendorName = e.VendorId.HasValue ? vendors.FirstOrDefault(v => v.Id == e.VendorId)?.Name : null,
            Category = e.Category,
            Amount = e.Amount,
            Description = e.Description,
            ExpenseDate = e.ExpenseDate
        }).ToList();
    }
}
