using CarRent.Domain.Entities;

namespace CarRent.Application.Interfaces;

public interface IBookingNotificationService
{
    Task NotifyBookingCreatedAsync(Booking booking, CancellationToken cancellationToken = default);
    Task NotifyBookingApprovedAsync(Booking booking, CancellationToken cancellationToken = default);
    Task NotifyBookingRejectedAsync(Booking booking, CancellationToken cancellationToken = default);
    Task NotifyBookingCancelledAsync(Booking booking, CancellationToken cancellationToken = default);
    Task NotifyBookingExtendedAsync(Booking booking, CancellationToken cancellationToken = default);
    Task NotifyWaitlistSlotAvailableAsync(WaitlistEntry entry, CancellationToken cancellationToken = default);
}
