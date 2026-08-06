using CarRent.Application.DTOs.Waitlist;

namespace CarRent.Application.Interfaces;

public interface IWaitlistService
{
    Task<IEnumerable<WaitlistDto>> GetAllAsync(WaitlistFilter filter, CancellationToken cancellationToken = default);
    Task<WaitlistDto> CreateAsync(Guid customerId, CreateWaitlistRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default);

    /// <summary>Finds Waiting entries matching the freed vehicle/category and overlapping date range, marks them Notified, and fires notifications.</summary>
    Task NotifyMatchingEntriesAsync(Guid vehicleId, DateTime freedStart, DateTime freedEnd, CancellationToken cancellationToken = default);
}
