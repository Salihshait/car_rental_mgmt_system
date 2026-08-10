using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class FraudDetectionService : IFraudDetectionService
{
    private const int AlertThreshold = 30;
    private static readonly string[] ExcludedBookingStatuses = { "Cancelled", "Rejected" };

    private readonly CarRentDbContext _context;

    public FraudDetectionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<FraudAlertDto?> EvaluateBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        var reasons = new List<string>();
        var score = 0;

        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == booking.CustomerId, cancellationToken);
        if (customer?.IsBlacklisted == true)
        {
            score += 40;
            reasons.Add("Customer is blacklisted.");
        }

        var windowStart = booking.BookingDate.AddHours(-24);
        var recentBookingCount = await _context.Bookings.AsNoTracking()
            .CountAsync(b => b.CustomerId == booking.CustomerId && b.BookingDate >= windowStart && b.BookingDate <= booking.BookingDate, cancellationToken);
        if (recentBookingCount >= 3)
        {
            score += 25;
            reasons.Add($"{recentBookingCount} bookings from this customer within 24 hours.");
        }

        var priorCompletedCount = await _context.Bookings.AsNoTracking()
            .CountAsync(b => b.CustomerId == booking.CustomerId && b.Status == "Completed" && b.Id != booking.Id, cancellationToken);
        var avgAmount = await _context.Bookings.AsNoTracking()
            .Where(b => !ExcludedBookingStatuses.Contains(b.Status))
            .AverageAsync(b => (decimal?)b.TotalAmount, cancellationToken) ?? 0;
        if (priorCompletedCount == 0 && avgAmount > 0 && booking.TotalAmount > avgAmount * 2)
        {
            score += 25;
            reasons.Add("High-value booking from a customer with no completed booking history.");
        }

        if (booking.CancelledAt.HasValue && (booking.CancelledAt.Value - booking.BookingDate).TotalMinutes < 10)
        {
            score += 10;
            reasons.Add("Booking was cancelled within minutes of creation.");
        }

        score = Math.Min(100, score);
        if (score < AlertThreshold)
        {
            return null;
        }

        var existing = await _context.FraudAlerts.FirstOrDefaultAsync(a => a.BookingId == booking.Id && a.Status == "Open", cancellationToken);
        if (existing is not null)
        {
            existing.RiskScore = score;
            existing.Reasons = string.Join(" ", reasons);
            await _context.SaveChangesAsync(cancellationToken);
            return await ToDtoAsync(existing, cancellationToken);
        }

        var alert = new FraudAlert
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            RiskScore = score,
            Reasons = string.Join(" ", reasons),
            Status = "Open"
        };

        await _context.FraudAlerts.AddAsync(alert, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(alert, cancellationToken);
    }

    public async Task<IEnumerable<FraudAlertDto>> GetAlertsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.FraudAlerts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);

        var alerts = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        var userIds = alerts.Select(a => a.CustomerId).Distinct().ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        return alerts.Select(a => MapDto(a, users.FirstOrDefault(u => u.Id == a.CustomerId)));
    }

    public async Task<FraudAlertDto> ReviewAlertAsync(Guid alertId, Guid reviewerId, ReviewFraudAlertRequest request, CancellationToken cancellationToken = default)
    {
        var alert = await _context.FraudAlerts.FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken)
            ?? throw new InvalidOperationException("Fraud alert not found.");

        alert.Status = request.Status;
        alert.ReviewedBy = reviewerId;
        alert.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(alert, cancellationToken);
    }

    private async Task<FraudAlertDto> ToDtoAsync(FraudAlert alert, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == alert.CustomerId, cancellationToken);
        return MapDto(alert, user);
    }

    private static FraudAlertDto MapDto(FraudAlert alert, User? user) => new(
        alert.Id, alert.BookingId, alert.PaymentId, alert.CustomerId,
        user is null ? null : $"{user.FirstName} {user.LastName}".Trim(),
        alert.RiskScore, alert.Reasons, alert.Status, alert.CreatedAt, alert.ReviewedBy, alert.ReviewedAt);
}
