namespace CarRent.Application.DTOs.Finance;

public record BankAccountDto(Guid Id, string Name, string AccountNumber, string BankName, Guid? BranchId, string? BranchName, decimal OpeningBalance, decimal CurrentBalance, bool IsActive, DateTime CreatedAt);

public record CreateBankAccountRequest(string Name, string AccountNumber, string BankName, Guid? BranchId, decimal OpeningBalance);

public record UpdateBankAccountRequest(string Name, string AccountNumber, string BankName, Guid? BranchId, bool IsActive);

public record BankTransactionDto(Guid Id, Guid BankAccountId, DateTime TransactionDate, string Type, decimal Amount, string Category, string? Description, decimal RunningBalance, DateTime CreatedAt);

public record CreateBankTransactionRequest(DateTime TransactionDate, string Type, decimal Amount, string Category, string? Description);
