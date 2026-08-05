using CarRent.Application.DTOs.Payments;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly CarRentDbContext _context;

    public PaymentService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Select(p => new PaymentSummaryDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Gateway = p.Gateway,
                Status = p.Status,
                TransactionReference = p.TransactionReference,
                PaidAt = p.PaidAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PaymentSummaryDto
            {
                Id = p.Id,
                BookingId = p.BookingId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Gateway = p.Gateway,
                Status = p.Status,
                TransactionReference = p.TransactionReference,
                PaidAt = p.PaidAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentSummaryDto> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var bookingExists = await _context.Bookings.AnyAsync(b => b.Id == request.BookingId, cancellationToken);
        if (!bookingExists)
        {
            throw new InvalidOperationException("The payment booking reference is invalid.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Gateway = request.Gateway,
            Status = request.Status,
            TransactionReference = request.TransactionReference,
            PaidAt = request.PaidAt ?? DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment, cancellationToken);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == request.BookingId, cancellationToken);
        if (invoice is not null)
        {
            invoice.Status = "Paid";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentSummaryDto
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Gateway = payment.Gateway,
            Status = payment.Status,
            TransactionReference = payment.TransactionReference,
            PaidAt = payment.PaidAt
        };
    }
}
