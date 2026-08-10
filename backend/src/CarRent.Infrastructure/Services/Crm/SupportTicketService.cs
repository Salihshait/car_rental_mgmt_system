using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class SupportTicketService : ISupportTicketService
{
    private readonly CarRentDbContext _context;

    public SupportTicketService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicketDto> CreateAsync(Guid customerId, CreateSupportTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BookingId = request.BookingId,
            Subject = request.Subject,
            Category = request.Category,
            Priority = request.Priority,
            Status = "Open"
        };

        await _context.SupportTickets.AddAsync(ticket, cancellationToken);
        await _context.SupportTicketMessages.AddAsync(new SupportTicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            SenderUserId = customerId,
            Message = request.Message
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(ticket, cancellationToken);
    }

    public async Task<IEnumerable<SupportTicketDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var tickets = await _context.SupportTickets.AsNoTracking()
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return await ToDtosAsync(tickets, cancellationToken);
    }

    public async Task<IEnumerable<SupportTicketDto>> GetAllAsync(string? status, string? priority, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportTickets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = query.Where(t => t.Priority == priority);
        }

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
        return await ToDtosAsync(tickets, cancellationToken);
    }

    public async Task<SupportTicketDetailDto> GetDetailAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.SupportTickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Support ticket not found.");

        var messages = await _context.SupportTicketMessages.AsNoTracking()
            .Where(m => m.TicketId == ticketId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = messages.Select(m => m.SenderUserId).Distinct().ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);

        var messageDtos = messages.Select(m =>
        {
            var user = users.FirstOrDefault(u => u.Id == m.SenderUserId);
            return new TicketMessageDto(m.Id, m.TicketId, m.SenderUserId, user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim(), m.IsInternalNote, m.Message, m.CreatedAt);
        }).ToList();

        var ticketDto = await ToDtoAsync(ticket, cancellationToken);
        return new SupportTicketDetailDto(ticketDto, messageDtos);
    }

    public async Task<TicketMessageDto> AddMessageAsync(Guid ticketId, Guid senderUserId, AddTicketMessageRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Support ticket not found.");

        var message = new SupportTicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SenderUserId = senderUserId,
            IsInternalNote = request.IsInternalNote,
            Message = request.Message
        };

        await _context.SupportTicketMessages.AddAsync(message, cancellationToken);

        if (ticket.Status == "Resolved" || ticket.Status == "Closed")
        {
            ticket.Status = "Open";
        }

        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == senderUserId, cancellationToken);
        return new TicketMessageDto(message.Id, message.TicketId, message.SenderUserId, user is null ? "Unknown" : $"{user.FirstName} {user.LastName}".Trim(), message.IsInternalNote, message.Message, message.CreatedAt);
    }

    public async Task AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Support ticket not found.");

        ticket.AssignedToUserId = request.AssignedToUserId;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid ticketId, UpdateTicketStatusRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new InvalidOperationException("Support ticket not found.");

        ticket.Status = request.Status;
        ticket.ResolvedAt = request.Status is "Resolved" or "Closed" ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<SupportTicketDto> ToDtoAsync(SupportTicket ticket, CancellationToken cancellationToken)
    {
        var userIds = new List<Guid> { ticket.CustomerId };
        if (ticket.AssignedToUserId.HasValue) userIds.Add(ticket.AssignedToUserId.Value);
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
        return MapDto(ticket, users);
    }

    private async Task<List<SupportTicketDto>> ToDtosAsync(List<SupportTicket> tickets, CancellationToken cancellationToken)
    {
        var userIds = tickets.Select(t => t.CustomerId)
            .Concat(tickets.Where(t => t.AssignedToUserId.HasValue).Select(t => t.AssignedToUserId!.Value))
            .Distinct()
            .ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
        return tickets.Select(t => MapDto(t, users)).ToList();
    }

    private static SupportTicketDto MapDto(SupportTicket ticket, List<User> users)
    {
        var customer = users.FirstOrDefault(u => u.Id == ticket.CustomerId);
        var assignee = ticket.AssignedToUserId.HasValue ? users.FirstOrDefault(u => u.Id == ticket.AssignedToUserId) : null;
        return new SupportTicketDto(
            ticket.Id,
            ticket.CustomerId,
            customer is null ? null : $"{customer.FirstName} {customer.LastName}".Trim(),
            ticket.BookingId,
            ticket.Subject,
            ticket.Category,
            ticket.Priority,
            ticket.Status,
            ticket.AssignedToUserId,
            assignee is null ? null : $"{assignee.FirstName} {assignee.LastName}".Trim(),
            ticket.CreatedAt,
            ticket.ResolvedAt);
    }
}
