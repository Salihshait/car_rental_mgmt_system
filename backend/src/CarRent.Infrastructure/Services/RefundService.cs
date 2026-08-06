using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class RefundService : IRefundService
{
    private static readonly string[] GatewayMethods = { "Razorpay", "Stripe" };

    private readonly CarRentDbContext _context;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IPaymentService _paymentService;

    public RefundService(CarRentDbContext context, IPaymentGatewayService gatewayService, IPaymentService paymentService)
    {
        _context = context;
        _gatewayService = gatewayService;
        _paymentService = paymentService;
    }

    public async Task<IEnumerable<RefundDto>> GetAllAsync(Guid? bookingId, CancellationToken cancellationToken = default)
    {
        var query = _context.Refunds.AsNoTracking().AsQueryable();

        if (bookingId.HasValue)
        {
            query = query.Where(r => r.BookingId == bookingId);
        }

        return await query.OrderByDescending(r => r.RequestedAt).Select(r => ToDto(r)).ToListAsync(cancellationToken);
    }

    public async Task<RefundDto> CreateAsync(CreateRefundRequest request, Guid requestedBy, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Refund amount must be greater than zero.");
        }

        if (!await _context.Bookings.AnyAsync(b => b.Id == request.BookingId, cancellationToken))
        {
            throw new InvalidOperationException("Booking not found.");
        }

        if (request.PaymentId.HasValue)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.BookingId == request.BookingId, cancellationToken)
                ?? throw new InvalidOperationException("Payment not found for this booking.");

            if (payment.Status != "Verified")
            {
                throw new InvalidOperationException("Only verified payments can be refunded.");
            }
        }

        var refund = new Refund
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            PaymentId = request.PaymentId,
            Amount = request.Amount,
            Reason = request.Reason,
            RefundMethod = request.RefundMethod,
            Status = "Requested",
            RequestedBy = requestedBy
        };

        await _context.Refunds.AddAsync(refund, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(refund);
    }

    public async Task<RefundDto> ApproveAsync(Guid id, Guid processedBy, CancellationToken cancellationToken = default)
    {
        var refund = await _context.Refunds.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Refund not found.");

        if (refund.Status != "Requested")
        {
            throw new InvalidOperationException("Only requested refunds can be approved.");
        }

        Payment? payment = null;
        if (refund.PaymentId.HasValue)
        {
            payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == refund.PaymentId, cancellationToken)
                ?? throw new InvalidOperationException("The refund's payment record could not be found.");

            refund.Gateway = payment.Gateway;

            if (GatewayMethods.Contains(payment.Gateway))
            {
                var result = await _gatewayService.RefundAsync(payment.Gateway, payment.TransactionReference ?? payment.Id.ToString(), refund.Amount, cancellationToken);
                refund.GatewayRefundReference = result.RefundReference;
            }
        }

        refund.Status = "Processed";
        refund.ProcessedBy = processedBy;
        refund.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (payment?.InvoiceId is not null)
        {
            await _paymentService.RecalculateInvoiceStatusAsync(payment.InvoiceId.Value, cancellationToken);
        }

        return ToDto(refund);
    }

    public async Task<RefundDto> RejectAsync(Guid id, RejectRefundRequest request, Guid processedBy, CancellationToken cancellationToken = default)
    {
        var refund = await _context.Refunds.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Refund not found.");

        if (refund.Status != "Requested")
        {
            throw new InvalidOperationException("Only requested refunds can be rejected.");
        }

        refund.Status = "Rejected";
        refund.Reason = request.Reason ?? refund.Reason;
        refund.ProcessedBy = processedBy;
        refund.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(refund);
    }

    private static RefundDto ToDto(Refund r) => new()
    {
        Id = r.Id,
        BookingId = r.BookingId,
        PaymentId = r.PaymentId,
        Amount = r.Amount,
        Reason = r.Reason,
        RefundMethod = r.RefundMethod,
        Gateway = r.Gateway,
        GatewayRefundReference = r.GatewayRefundReference,
        Status = r.Status,
        RequestedAt = r.RequestedAt,
        ProcessedAt = r.ProcessedAt
    };
}
