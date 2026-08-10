using CarRent.Application.DTOs.Crm;

namespace CarRent.Application.Interfaces;

public interface ISupportTicketService
{
    Task<SupportTicketDto> CreateAsync(Guid customerId, CreateSupportTicketRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupportTicketDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SupportTicketDto>> GetAllAsync(string? status, string? priority, CancellationToken cancellationToken = default);
    Task<SupportTicketDetailDto> GetDetailAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<TicketMessageDto> AddMessageAsync(Guid ticketId, Guid senderUserId, AddTicketMessageRequest request, CancellationToken cancellationToken = default);
    Task AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CancellationToken cancellationToken = default);
}
