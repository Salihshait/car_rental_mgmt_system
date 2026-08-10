using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class FeedbackService : IFeedbackService
{
    private readonly CarRentDbContext _context;

    public FeedbackService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<FeedbackDto> CreateAsync(Guid customerId, CreateFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Rating is < 1 or > 5)
        {
            throw new InvalidOperationException("Rating must be between 1 and 5.");
        }

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BookingId = request.BookingId,
            Rating = request.Rating,
            Comment = request.Comment,
            Category = request.Category
        };

        await _context.FeedbackEntries.AddAsync(feedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(feedback, cancellationToken);
    }

    public async Task<IEnumerable<FeedbackDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var feedback = await _context.FeedbackEntries.AsNoTracking()
            .Where(f => f.CustomerId == customerId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        return await ToDtosAsync(feedback, cancellationToken);
    }

    public async Task<IEnumerable<FeedbackDto>> GetAllAsync(string? category, bool? isPublished, CancellationToken cancellationToken = default)
    {
        var query = _context.FeedbackEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(f => f.Category == category);
        }
        if (isPublished.HasValue)
        {
            query = query.Where(f => f.IsPublished == isPublished.Value);
        }

        var feedback = await query.OrderByDescending(f => f.CreatedAt).ToListAsync(cancellationToken);
        return await ToDtosAsync(feedback, cancellationToken);
    }

    public async Task<FeedbackDto> SetPublishedAsync(Guid id, PublishFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        var feedback = await _context.FeedbackEntries.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Feedback not found.");

        feedback.IsPublished = request.IsPublished;
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(feedback, cancellationToken);
    }

    private async Task<FeedbackDto> ToDtoAsync(Feedback feedback, CancellationToken cancellationToken)
    {
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == feedback.CustomerId, cancellationToken);
        return MapDto(feedback, customer);
    }

    private async Task<List<FeedbackDto>> ToDtosAsync(List<Feedback> feedback, CancellationToken cancellationToken)
    {
        var userIds = feedback.Select(f => f.CustomerId).Distinct().ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
        return feedback.Select(f => MapDto(f, users.FirstOrDefault(u => u.Id == f.CustomerId))).ToList();
    }

    private static FeedbackDto MapDto(Feedback feedback, User? customer) => new(
        feedback.Id,
        feedback.CustomerId,
        customer is null ? null : $"{customer.FirstName} {customer.LastName}".Trim(),
        feedback.BookingId,
        feedback.Rating,
        feedback.Comment,
        feedback.Category,
        feedback.IsPublished,
        feedback.CreatedAt);
}
