using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class BookingNotificationService : IBookingNotificationService
{
    private readonly CarRentDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public BookingNotificationService(
        CarRentDbContext context,
        INotificationService notificationService,
        IEmailService emailService,
        ISmsService smsService)
    {
        _context = context;
        _notificationService = notificationService;
        _emailService = emailService;
        _smsService = smsService;
    }

    public Task NotifyBookingCreatedAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var message = booking.Status == "PendingApproval"
            ? $"Your booking request for {booking.StartDate:d} - {booking.EndDate:d} has been received and is awaiting approval."
            : $"Your booking for {booking.StartDate:d} - {booking.EndDate:d} is confirmed. Total: {booking.TotalAmount:C}.";

        return NotifyAsync(booking.CustomerId, "Booking", message, "Booking Update", cancellationToken);
    }

    public Task NotifyBookingApprovedAsync(Booking booking, CancellationToken cancellationToken = default) =>
        NotifyAsync(booking.CustomerId, "Booking", $"Your booking for {booking.StartDate:d} - {booking.EndDate:d} has been approved and confirmed.", "Booking Approved", cancellationToken);

    public Task NotifyBookingRejectedAsync(Booking booking, CancellationToken cancellationToken = default) =>
        NotifyAsync(booking.CustomerId, "Booking", $"Your booking request for {booking.StartDate:d} - {booking.EndDate:d} was not approved.{(string.IsNullOrWhiteSpace(booking.CancellationReason) ? "" : $" Reason: {booking.CancellationReason}")}", "Booking Rejected", cancellationToken);

    public Task NotifyBookingCancelledAsync(Booking booking, CancellationToken cancellationToken = default) =>
        NotifyAsync(booking.CustomerId, "Booking", $"Your booking for {booking.StartDate:d} - {booking.EndDate:d} has been cancelled.{(string.IsNullOrWhiteSpace(booking.CancellationReason) ? "" : $" Reason: {booking.CancellationReason}")}", "Booking Cancelled", cancellationToken);

    public Task NotifyBookingExtendedAsync(Booking booking, CancellationToken cancellationToken = default) =>
        NotifyAsync(booking.CustomerId, "Booking", $"Your booking has been extended. New return date: {booking.EndDate:d}. Updated total: {booking.TotalAmount:C}.", "Booking Extended", cancellationToken);

    public Task NotifyWaitlistSlotAvailableAsync(WaitlistEntry entry, CancellationToken cancellationToken = default) =>
        NotifyAsync(entry.CustomerId, "Waitlist", $"A vehicle matching your waitlist request for {entry.DesiredStartDate:d} - {entry.DesiredEndDate:d} is now available. Book soon to secure it.", "Vehicle Available", cancellationToken);

    private async Task NotifyAsync(Guid userId, string notificationType, string message, string emailSubject, CancellationToken cancellationToken)
    {
        await _notificationService.CreateAsync(userId, notificationType, message, cancellationToken);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        await _emailService.SendAsync(user.Email, emailSubject, message, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            await _smsService.SendAsync(user.PhoneNumber, message, cancellationToken);
        }
    }
}
