namespace CarRent.Application.DTOs.Finance;

public record JournalEntryDto(Guid Id, DateTime EntryDate, string EntryType, string Category, string Description, decimal Amount, Guid? BankAccountId, DateTime CreatedAt);

public record CreateJournalEntryRequest(DateTime EntryDate, string EntryType, string Category, string Description, decimal Amount, Guid? BankAccountId);
