namespace CarRent.Application.DTOs.Finance;

public record FinanceTransactionDto(DateTime Date, string Type, string Category, string Description, decimal Amount, string Source, Guid? SourceId);
