namespace CarRent.Application.Interfaces;

public interface ICampaignExecutionService
{
    Task SendNowAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
