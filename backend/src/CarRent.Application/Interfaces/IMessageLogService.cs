using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface IMessageLogService
{
    Task<IEnumerable<MessageLogDto>> GetAllAsync(string? channel, string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<MessageLogDto> SendAdHocAsync(SendAdHocMessageRequest request, CancellationToken cancellationToken = default);
}
