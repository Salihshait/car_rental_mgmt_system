using CarRent.Application.DTOs.Payments;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private static readonly string[] GatewayOnlyMethods = { "Razorpay", "Stripe" };
    private static readonly string[] ManualMethods = { "Cash", "UPI" };

    private readonly CarRentDbContext _context;
    private readonly IPaymentGatewayService _gatewayService;

    public PaymentService(CarRentDbContext context, IPaymentGatewayService gatewayService)
    {
        _context = context;
        _gatewayService = gatewayService;
    }

    public async Task<IEnumerable<PaymentSummaryDto>> GetAllAsync(Guid? bookingId, Guid? invoiceId, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.AsNoTracking().AsQueryable();

        if (bookingId.HasValue)
        {
            query = query.Where(p => p.BookingId == bookingId);
        }

        if (invoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == invoiceId);
        }

        return await query.OrderByDescending(p => p.CreatedAt).Select(p => ToDto(p)).ToListAsync(cancellationToken);
    }

    public async Task<PaymentSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return payment is null ? null : ToDto(payment);
    }

    public async Task<PaymentOrderDto> InitiateAsync(InitiatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (!GatewayOnlyMethods.Contains(request.Gateway))
        {
            throw new InvalidOperationException("Only Razorpay or Stripe can be used to create a payment order. Use the manual endpoint for Cash/UPI.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (request.InvoiceId.HasValue && !await _context.Invoices.AnyAsync(i => i.Id == request.InvoiceId && i.BookingId == booking.Id, cancellationToken))
        {
            throw new InvalidOperationException("Invoice not found for this booking.");
        }

        var order = await _gatewayService.CreateOrderAsync(request.Gateway, request.Amount, request.Currency, booking.Id.ToString(), cancellationToken);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethod = "Card",
            Gateway = request.Gateway,
            Status = "Created",
            GatewayOrderId = order.OrderId
        };

        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentOrderDto
        {
            PaymentId = payment.Id,
            GatewayOrderId = order.OrderId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Gateway = payment.Gateway,
            Status = payment.Status
        };
    }

    public async Task<PaymentSummaryDto> ConfirmAsync(Guid paymentId, ConfirmPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.Status != "Created")
        {
            throw new InvalidOperationException("This payment has already been processed.");
        }

        var result = await _gatewayService.VerifyPaymentAsync(payment.Gateway, payment.GatewayOrderId ?? string.Empty, request.GatewayPaymentReference, request.Signature, cancellationToken);

        if (!result.IsVerified)
        {
            payment.Status = "Failed";
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException(result.Message ?? "Payment verification failed.");
        }

        payment.Status = "Verified";
        payment.TransactionReference = request.GatewayPaymentReference;
        payment.GatewaySignature = request.Signature;
        payment.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        if (payment.InvoiceId.HasValue)
        {
            await RecalculateInvoiceStatusAsync(payment.InvoiceId.Value, cancellationToken);
        }

        return ToDto(payment);
    }

    public async Task<PaymentSummaryDto> RecordManualPaymentAsync(RecordManualPaymentRequest request, Guid recordedBy, CancellationToken cancellationToken = default)
    {
        if (!ManualMethods.Contains(request.PaymentMethod))
        {
            throw new InvalidOperationException("PaymentMethod must be 'Cash' or 'UPI' for manual payments.");
        }

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Amount must be greater than zero.");
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (request.InvoiceId.HasValue && !await _context.Invoices.AnyAsync(i => i.Id == request.InvoiceId && i.BookingId == booking.Id, cancellationToken))
        {
            throw new InvalidOperationException("Invoice not found for this booking.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Gateway = request.PaymentMethod,
            Status = "Verified",
            TransactionReference = request.TransactionReference,
            PaidAt = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment, cancellationToken);

        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = recordedBy,
            Action = "ManualPaymentRecorded",
            EntityType = "Booking",
            EntityId = booking.Id,
            Payload = $"{{\"message\":\"{request.PaymentMethod} payment of {request.Amount} recorded.\"}}"
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (payment.InvoiceId.HasValue)
        {
            await RecalculateInvoiceStatusAsync(payment.InvoiceId.Value, cancellationToken);
        }

        return ToDto(payment);
    }

    public async Task RecalculateInvoiceStatusAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        var paymentsTotal = await _context.Payments
            .Where(p => p.InvoiceId == invoiceId && p.Status == "Verified")
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var refundedTotal = await (
            from r in _context.Refunds
            join p in _context.Payments on r.PaymentId equals p.Id
            where p.InvoiceId == invoiceId && r.Status == "Processed"
            select r.Amount).SumAsync(cancellationToken);

        var amountPaid = Math.Max(0, paymentsTotal - refundedTotal);
        invoice.AmountPaid = amountPaid;
        invoice.Status = amountPaid <= 0 ? "Unpaid" : amountPaid >= invoice.TotalAmount ? "Paid" : "PartiallyPaid";

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static PaymentSummaryDto ToDto(Payment p) => new()
    {
        Id = p.Id,
        BookingId = p.BookingId,
        InvoiceId = p.InvoiceId,
        Amount = p.Amount,
        Currency = p.Currency,
        Purpose = p.Purpose,
        PaymentMethod = p.PaymentMethod,
        Gateway = p.Gateway,
        Status = p.Status,
        TransactionReference = p.TransactionReference,
        GatewayOrderId = p.GatewayOrderId,
        PaidAt = p.PaidAt,
        CreatedAt = p.CreatedAt
    };
}
