using CarRent.Application.DTOs.Ai;

namespace CarRent.Application.Interfaces;

public interface IChatbotService
{
    Task<ChatSessionDto> StartSessionAsync(Guid? customerId, CancellationToken cancellationToken = default);
    Task<ChatMessageDto> SendMessageAsync(Guid sessionId, SendChatMessageRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
