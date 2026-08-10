using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface IFeedbackService
{
    Task<FeedbackDto> CreateAsync(Guid customerId, CreateFeedbackRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeedbackDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FeedbackDto>> GetAllAsync(string? category, bool? isPublished, CancellationToken cancellationToken = default);
    Task<FeedbackDto> SetPublishedAsync(Guid id, PublishFeedbackRequest request, CancellationToken cancellationToken = default);
}
