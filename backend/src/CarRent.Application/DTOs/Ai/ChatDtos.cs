namespace CarRent.Application.DTOs.Ai;

public record ChatMessageDto(Guid Id, Guid SessionId, string Sender, string Message, DateTime CreatedAt);

public record ChatSessionDto(Guid Id, Guid? CustomerId, string Channel, DateTime StartedAt, DateTime LastMessageAt);

public record SendChatMessageRequest(string Message);
