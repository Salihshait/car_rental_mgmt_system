using CarRent.Application.DTOs.Finance;

namespace CarRent.Application.Interfaces;

public interface IFinanceTransactionService
{
    Task<List<FinanceTransactionDto>> GetTransactionsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
