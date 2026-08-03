using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly CarRentDbContext _context;

    public BookingService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookingSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Select(b => new BookingSummaryDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                VehicleId = b.VehicleId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalAmount = b.TotalAmount,
                Status = b.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BookingSummaryDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                VehicleId = b.VehicleId,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalAmount = b.TotalAmount,
                Status = b.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BookingSummaryDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == request.VehicleId, cancellationToken);
        if (!vehicleExists)
        {
            throw new InvalidOperationException("The selected vehicle does not exist.");
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            BookingDate = DateTime.UtcNow,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalAmount = request.TotalAmount,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Confirmed" : request.Status,
            Notes = request.Notes
        };

        await _context.Bookings.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new BookingSummaryDto
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            VehicleId = booking.VehicleId,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status
        };
    }
}
