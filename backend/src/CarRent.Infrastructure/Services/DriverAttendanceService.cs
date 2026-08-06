using CarRent.Application.DTOs.Drivers;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class DriverAttendanceService : IDriverAttendanceService
{
    private readonly CarRentDbContext _context;

    public DriverAttendanceService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<DriverAttendanceDto> CheckInAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.DriverAttendances.FirstOrDefaultAsync(a => a.DriverId == driverId && a.AttendanceDate == today, cancellationToken);

        if (record is not null && record.CheckInAt is not null)
        {
            throw new InvalidOperationException("You have already checked in today.");
        }

        if (record is null)
        {
            record = new DriverAttendance { Id = Guid.NewGuid(), DriverId = driverId, AttendanceDate = today };
            await _context.DriverAttendances.AddAsync(record, cancellationToken);
        }

        record.CheckInAt = DateTime.UtcNow;
        record.Status = "Present";

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(record);
    }

    public async Task<DriverAttendanceDto> CheckOutAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.DriverAttendances.FirstOrDefaultAsync(a => a.DriverId == driverId && a.AttendanceDate == today, cancellationToken);

        if (record?.CheckInAt is null)
        {
            throw new InvalidOperationException("You must check in before checking out.");
        }

        if (record.CheckOutAt is not null)
        {
            throw new InvalidOperationException("You have already checked out today.");
        }

        record.CheckOutAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(record);
    }

    public async Task<IEnumerable<DriverAttendanceDto>> GetAttendanceAsync(Guid driverId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _context.DriverAttendances.AsNoTracking().Where(a => a.DriverId == driverId);

        if (from.HasValue)
        {
            query = query.Where(a => a.AttendanceDate >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(a => a.AttendanceDate <= to.Value.Date);
        }

        return await query.OrderByDescending(a => a.AttendanceDate).Select(a => ToDto(a)).ToListAsync(cancellationToken);
    }

    public async Task<DriverAttendanceDto> MarkAsync(Guid driverId, MarkAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var date = request.AttendanceDate.Date;
        var record = await _context.DriverAttendances.FirstOrDefaultAsync(a => a.DriverId == driverId && a.AttendanceDate == date, cancellationToken);

        if (record is null)
        {
            record = new DriverAttendance { Id = Guid.NewGuid(), DriverId = driverId, AttendanceDate = date };
            await _context.DriverAttendances.AddAsync(record, cancellationToken);
        }

        record.Status = request.Status;
        record.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);
        return ToDto(record);
    }

    private static DriverAttendanceDto ToDto(DriverAttendance a) => new()
    {
        Id = a.Id,
        DriverId = a.DriverId,
        AttendanceDate = a.AttendanceDate,
        CheckInAt = a.CheckInAt,
        CheckOutAt = a.CheckOutAt,
        Status = a.Status,
        Notes = a.Notes
    };
}
