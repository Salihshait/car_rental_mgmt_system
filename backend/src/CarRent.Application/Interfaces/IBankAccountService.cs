using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IBankAccountService
{
    Task<IEnumerable<BankAccountDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BankAccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BankAccountDto> CreateAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<BankAccountDto> UpdateAsync(Guid id, UpdateBankAccountRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<BankTransactionDto>> GetTransactionsAsync(Guid bankAccountId, CancellationToken cancellationToken = default);
    Task<BankTransactionDto> AddTransactionAsync(Guid bankAccountId, Guid createdByUserId, CreateBankTransactionRequest request, CancellationToken cancellationToken = default);
}
