using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Crm;

public class CampaignSchedulerHostedService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignSchedulerHostedService> _logger;

    public CampaignSchedulerHostedService(IServiceScopeFactory scopeFactory, ILogger<CampaignSchedulerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunDueCampaignsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Campaign scheduler poll failed.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }

    private async Task RunDueCampaignsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CarRentDbContext>();
        var executionService = scope.ServiceProvider.GetRequiredService<ICampaignExecutionService>();

        var now = DateTime.UtcNow;
        var dueCampaignIds = await context.Campaigns.AsNoTracking()
            .Where(c => c.Status == "Scheduled" && c.ScheduledAt != null && c.ScheduledAt <= now)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        foreach (var campaignId in dueCampaignIds)
        {
            await executionService.SendNowAsync(campaignId, cancellationToken);
        }
    }
}
