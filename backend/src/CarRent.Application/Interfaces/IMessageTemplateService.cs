using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface IMessageTemplateService
{
    Task<IEnumerable<MessageTemplateDto>> GetAllAsync(string? channel, CancellationToken cancellationToken = default);
    Task<MessageTemplateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MessageTemplateDto> CreateAsync(UpsertTemplateRequest request, CancellationToken cancellationToken = default);
    Task<MessageTemplateDto> UpdateAsync(Guid id, UpsertTemplateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TemplatePreviewResult> PreviewAsync(Guid id, TemplatePreviewRequest request, CancellationToken cancellationToken = default);
}
