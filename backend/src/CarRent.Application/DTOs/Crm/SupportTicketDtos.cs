namespace CarRent.Application.DTOs.Crm;

public record SupportTicketDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    Guid? BookingId,
    string Subject,
    string Category,
    string Priority,
    string Status,
    Guid? AssignedToUserId,
    string? AssignedToName,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public record TicketMessageDto(Guid Id, Guid TicketId, Guid SenderUserId, string SenderName, bool IsInternalNote, string Message, DateTime CreatedAt);

public record SupportTicketDetailDto(SupportTicketDto Ticket, List<TicketMessageDto> Messages);

public record CreateSupportTicketRequest(string Subject, string Category, string Priority, Guid? BookingId, string Message);

public record AddTicketMessageRequest(string Message, bool IsInternalNote = false);

public record UpdateTicketStatusRequest(string Status);

public record AssignTicketRequest(Guid? AssignedToUserId);
