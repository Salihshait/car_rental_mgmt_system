using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class ChatbotService : IChatbotService
{
    private readonly CarRentDbContext _context;
    private readonly IChatbotProvider _provider;

    public ChatbotService(CarRentDbContext context, IChatbotProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<ChatSessionDto> StartSessionAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Channel = "Web"
        };

        await _context.ChatSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChatSessionDto(session.Id, session.CustomerId, session.Channel, session.StartedAt, session.LastMessageAt);
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid sessionId, SendChatMessageRequest request, CancellationToken cancellationToken = default)
    {
        var session = await _context.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Chat session not found.");

        var customerMessage = new ChatMessage { Id = Guid.NewGuid(), SessionId = sessionId, Sender = "Customer", Message = request.Message };
        await _context.ChatMessages.AddAsync(customerMessage, cancellationToken);

        var history = await _context.ChatMessages.AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.Message)
            .ToListAsync(cancellationToken);

        var replyText = await _provider.GenerateReplyAsync(history, request.Message, cancellationToken);

        var botMessage = new ChatMessage { Id = Guid.NewGuid(), SessionId = sessionId, Sender = "Bot", Message = replyText };
        await _context.ChatMessages.AddAsync(botMessage, cancellationToken);

        session.LastMessageAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ChatMessageDto(botMessage.Id, botMessage.SessionId, botMessage.Sender, botMessage.Message, botMessage.CreatedAt);
    }

    public async Task<IEnumerable<ChatMessageDto>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var messages = await _context.ChatMessages.AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new ChatMessageDto(m.Id, m.SessionId, m.Sender, m.Message, m.CreatedAt));
    }
}
