namespace CarRent.Application.DTOs.Finance;

public record CashbookEntryDto(DateTime Date, string Description, string Category, string Type, decimal Amount, decimal RunningBalance);

public record LedgerAccountDto(string Account, decimal TotalIncome, decimal TotalExpense, decimal Net, List<FinanceTransactionDto> Entries);
