using CarRent.Application.DTOs.Crm;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Crm;

public class ComplaintService : IComplaintService
{
    private readonly CarRentDbContext _context;

    public ComplaintService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<ComplaintDto> CreateAsync(Guid customerId, CreateComplaintRequest request, CancellationToken cancellationToken = default)
    {
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BookingId = request.BookingId,
            VehicleId = request.VehicleId,
            Subject = request.Subject,
            Description = request.Description,
            Severity = request.Severity,
            Status = "Open"
        };

        await _context.Complaints.AddAsync(complaint, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(complaint, cancellationToken);
    }

    public async Task<IEnumerable<ComplaintDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var complaints = await _context.Complaints.AsNoTracking()
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return await ToDtosAsync(complaints, cancellationToken);
    }

    public async Task<IEnumerable<ComplaintDto>> GetAllAsync(string? status, string? severity, CancellationToken cancellationToken = default)
    {
        var query = _context.Complaints.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(c => c.Severity == severity);
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);
        return await ToDtosAsync(complaints, cancellationToken);
    }

    public async Task<ComplaintDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Complaint not found.");
        return await ToDtoAsync(complaint, cancellationToken);
    }

    public async Task<ComplaintDto> ResolveAsync(Guid id, ResolveComplaintRequest request, CancellationToken cancellationToken = default)
    {
        var complaint = await _context.Complaints.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Complaint not found.");

        complaint.Status = request.Status;
        complaint.Resolution = request.Resolution;
        complaint.ResolvedAt = request.Status is "Resolved" or "Rejected" ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(complaint, cancellationToken);
    }

    private async Task<ComplaintDto> ToDtoAsync(Complaint complaint, CancellationToken cancellationToken)
    {
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == complaint.CustomerId, cancellationToken);
        return MapDto(complaint, customer);
    }

    private async Task<List<ComplaintDto>> ToDtosAsync(List<Complaint> complaints, CancellationToken cancellationToken)
    {
        var userIds = complaints.Select(c => c.CustomerId).Distinct().ToList();
        var users = await _context.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
        return complaints.Select(c => MapDto(c, users.FirstOrDefault(u => u.Id == c.CustomerId))).ToList();
    }

    private static ComplaintDto MapDto(Complaint complaint, User? customer) => new(
        complaint.Id,
        complaint.CustomerId,
        customer is null ? null : $"{customer.FirstName} {customer.LastName}".Trim(),
        complaint.BookingId,
        complaint.VehicleId,
        complaint.Subject,
        complaint.Description,
        complaint.Severity,
        complaint.Status,
        complaint.Resolution,
        complaint.AssignedToUserId,
        complaint.CreatedAt,
        complaint.ResolvedAt);
}
