using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IJournalService
{
    Task<IEnumerable<JournalEntryDto>> GetAllAsync(DateTime? from, DateTime? to, string? entryType, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> CreateAsync(Guid createdByUserId, CreateJournalEntryRequest request, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> UpdateAsync(Guid id, CreateJournalEntryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
