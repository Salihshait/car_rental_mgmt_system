using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetAllAsync(string? status, CancellationToken cancellationToken = default);
    Task<CampaignDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CampaignDto> CreateAsync(Guid createdByUserId, CreateCampaignRequest request, CancellationToken cancellationToken = default);
    Task<CampaignDto> ScheduleAsync(Guid id, ScheduleCampaignRequest request, CancellationToken cancellationToken = default);
    Task<CampaignDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MessageLogDto>> GetLogsAsync(Guid id, CancellationToken cancellationToken = default);
}
