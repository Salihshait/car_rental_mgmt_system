using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class MessageTemplateService : IMessageTemplateService
{
    private static readonly Dictionary<string, string> SamplePlaceholders = new()
    {
        ["CustomerName"] = "Alex Rivera",
        ["BookingId"] = "BK-10234",
        ["VehicleName"] = "Toyota Corolla",
    };

    private readonly CarRentDbContext _context;

    public MessageTemplateService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MessageTemplateDto>> GetAllAsync(string? channel, CancellationToken cancellationToken = default)
    {
        var query = _context.MessageTemplates.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = query.Where(t => t.Channel == channel);
        }

        var templates = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);
        return templates.Select(MapDto);
    }

    public async Task<MessageTemplateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");
        return MapDto(template);
    }

    public async Task<MessageTemplateDto> CreateAsync(UpsertTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new MessageTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Channel = request.Channel,
            Subject = request.Subject,
            Body = request.Body,
            IsActive = request.IsActive
        };

        await _context.MessageTemplates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(template);
    }

    public async Task<MessageTemplateDto> UpdateAsync(Guid id, UpsertTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        template.Name = request.Name;
        template.Channel = request.Channel;
        template.Subject = request.Subject;
        template.Body = request.Body;
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return MapDto(template);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        _context.MessageTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TemplatePreviewResult> PreviewAsync(Guid id, TemplatePreviewRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Template not found.");

        var values = request.SampleValues is { Count: > 0 } ? request.SampleValues : SamplePlaceholders;
        var body = TemplateRenderer.Render(template.Body, values);
        var subject = template.Subject is null ? null : TemplateRenderer.Render(template.Subject, values);

        return new TemplatePreviewResult(subject, body);
    }

    private static MessageTemplateDto MapDto(MessageTemplate template) => new(
        template.Id, template.Name, template.Channel, template.Subject, template.Body, template.IsActive, template.CreatedAt, template.UpdatedAt);
}
