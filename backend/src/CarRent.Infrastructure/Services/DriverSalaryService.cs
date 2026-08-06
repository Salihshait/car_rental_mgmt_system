using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverSalaryService : IDriverSalaryService
{
    private readonly CarRentDbContext _context;

    public DriverSalaryService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DriverSalaryPaymentDto>> GetAllAsync(Guid? driverId, CancellationToken cancellationToken = default)
    {
        var query = _context.DriverSalaryPayments.AsNoTracking().AsQueryable();

        if (driverId.HasValue)
        {
            query = query.Where(s => s.DriverId == driverId);
        }

        return await query.OrderByDescending(s => s.PeriodStart).Select(s => ToDto(s)).ToListAsync(cancellationToken);
    }

    public async Task<DriverSalaryPaymentDto> GenerateAsync(CreateSalaryPaymentRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        if (!await _context.Drivers.AnyAsync(d => d.Id == request.DriverId, cancellationToken))
        {
            throw new InvalidOperationException("Driver not found.");
        }

        if (request.PeriodEnd <= request.PeriodStart)
        {
            throw new InvalidOperationException("Period end must be after the period start.");
        }

        var netAmount = request.BaseAmount - request.Deductions + request.Bonus;
        if (netAmount < 0)
        {
            throw new InvalidOperationException("Net amount cannot be negative.");
        }

        var payment = new DriverSalaryPayment
        {
            Id = Guid.NewGuid(),
            DriverId = request.DriverId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            BaseAmount = request.BaseAmount,
            Deductions = request.Deductions,
            Bonus = request.Bonus,
            NetAmount = netAmount,
            Status = "Pending",
            Notes = request.Notes,
            CreatedBy = createdBy
        };

        await _context.DriverSalaryPayments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(payment);
    }

    public async Task<DriverSalaryPaymentDto> MarkPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.DriverSalaryPayments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Salary payment not found.");

        if (payment.Status == "Paid")
        {
            throw new InvalidOperationException("This salary payment has already been marked paid.");
        }

        payment.Status = "Paid";
        payment.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(payment);
    }

    private static DriverSalaryPaymentDto ToDto(DriverSalaryPayment s) => new()
    {
        Id = s.Id,
        DriverId = s.DriverId,
        PeriodStart = s.PeriodStart,
        PeriodEnd = s.PeriodEnd,
        BaseAmount = s.BaseAmount,
        Deductions = s.Deductions,
        Bonus = s.Bonus,
        NetAmount = s.NetAmount,
        Status = s.Status,
        PaidAt = s.PaidAt,
        Notes = s.Notes
    };
}
