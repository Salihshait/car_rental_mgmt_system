namespace CarRent.Application.Interfaces;

public interface IMessageDispatchService
{
    Task<Guid> DispatchAsync(
        string channel,
        string recipientAddress,
        Guid? recipientUserId,
        string? subject,
        string body,
        Guid? templateId,
        Guid? campaignId,
        CancellationToken cancellationToken = default);
}
