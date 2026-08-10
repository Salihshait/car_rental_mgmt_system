using CarRent.Application.DTOs.Finance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class JournalService : IJournalService
{
    private readonly CarRentDbContext _context;

    public JournalService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JournalEntryDto>> GetAllAsync(DateTime? from, DateTime? to, string? entryType, CancellationToken cancellationToken = default)
    {
        var query = _context.JournalEntries.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(j => j.EntryDate >= from);
        if (to.HasValue) query = query.Where(j => j.EntryDate <= to);
        if (!string.IsNullOrWhiteSpace(entryType)) query = query.Where(j => j.EntryType == entryType);

        var entries = await query.OrderByDescending(j => j.EntryDate).ToListAsync(cancellationToken);
        return entries.Select(MapDto);
    }

    public async Task<JournalEntryDto> CreateAsync(Guid createdByUserId, CreateJournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryDate = request.EntryDate,
            EntryType = request.EntryType,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            BankAccountId = request.BankAccountId,
            CreatedBy = createdByUserId
        };

        await _context.JournalEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapDto(entry);
    }

    public async Task<JournalEntryDto> UpdateAsync(Guid id, CreateJournalEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Journal entry not found.");

        entry.EntryDate = request.EntryDate;
        entry.EntryType = request.EntryType;
        entry.Category = request.Category;
        entry.Description = request.Description;
        entry.Amount = request.Amount;
        entry.BankAccountId = request.BankAccountId;

        await _context.SaveChangesAsync(cancellationToken);
        return MapDto(entry);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entry = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Journal entry not found.");

        _context.JournalEntries.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static JournalEntryDto MapDto(JournalEntry entry) => new(
        entry.Id, entry.EntryDate, entry.EntryType, entry.Category, entry.Description, entry.Amount, entry.BankAccountId, entry.CreatedAt);
}
