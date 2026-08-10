using CarRent.Application.DTOs.Finance;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class BankAccountService : IBankAccountService
{
    private readonly CarRentDbContext _context;

    public BankAccountService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BankAccountDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.BankAccounts.AsNoTracking().OrderBy(a => a.Name).ToListAsync(cancellationToken);
        var branchIds = accounts.Where(a => a.BranchId.HasValue).Select(a => a.BranchId!.Value).Distinct().ToList();
        var branches = await _context.Branches.AsNoTracking().Where(b => branchIds.Contains(b.Id)).ToListAsync(cancellationToken);

        var accountIds = accounts.Select(a => a.Id).ToList();
        var transactions = await _context.BankTransactions.AsNoTracking().Where(t => accountIds.Contains(t.BankAccountId)).ToListAsync(cancellationToken);

        return accounts.Select(a => MapDto(a, branches.FirstOrDefault(b => b.Id == a.BranchId), transactions.Where(t => t.BankAccountId == a.Id)));
    }

    public async Task<BankAccountDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");
        var branch = account.BranchId.HasValue ? await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == account.BranchId, cancellationToken) : null;
        var transactions = await _context.BankTransactions.AsNoTracking().Where(t => t.BankAccountId == id).ToListAsync(cancellationToken);

        return MapDto(account, branch, transactions);
    }

    public async Task<BankAccountDto> CreateAsync(CreateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            AccountNumber = request.AccountNumber,
            BankName = request.BankName,
            BranchId = request.BranchId,
            OpeningBalance = request.OpeningBalance
        };

        await _context.BankAccounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(account.Id, cancellationToken);
    }

    public async Task<BankAccountDto> UpdateAsync(Guid id, UpdateBankAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");

        account.Name = request.Name;
        account.AccountNumber = request.AccountNumber;
        account.BankName = request.BankName;
        account.BranchId = request.BranchId;
        account.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<BankTransactionDto>> GetTransactionsAsync(Guid bankAccountId, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == bankAccountId, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");

        var transactions = await _context.BankTransactions.AsNoTracking()
            .Where(t => t.BankAccountId == bankAccountId)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        return BuildTransactionDtos(account, transactions).OrderByDescending(t => t.TransactionDate);
    }

    public async Task<BankTransactionDto> AddTransactionAsync(Guid bankAccountId, Guid createdByUserId, CreateBankTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == bankAccountId, cancellationToken)
            ?? throw new InvalidOperationException("Bank account not found.");

        var transaction = new BankTransaction
        {
            Id = Guid.NewGuid(),
            BankAccountId = bankAccountId,
            TransactionDate = request.TransactionDate,
            Type = request.Type,
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            CreatedBy = createdByUserId
        };

        await _context.BankTransactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var allTransactions = await _context.BankTransactions.AsNoTracking()
            .Where(t => t.BankAccountId == bankAccountId)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        return BuildTransactionDtos(account, allTransactions).First(t => t.Id == transaction.Id);
    }

    private static BankAccountDto MapDto(BankAccount account, Branch? branch, IEnumerable<BankTransaction> transactions)
    {
        var net = transactions.Sum(t => t.Type == "Credit" ? t.Amount : -t.Amount);
        return new BankAccountDto(
            account.Id, account.Name, account.AccountNumber, account.BankName,
            account.BranchId, branch?.Name, account.OpeningBalance, account.OpeningBalance + net,
            account.IsActive, account.CreatedAt);
    }

    private static List<BankTransactionDto> BuildTransactionDtos(BankAccount account, List<BankTransaction> orderedTransactions)
    {
        var running = account.OpeningBalance;
        var result = new List<BankTransactionDto>();
        foreach (var t in orderedTransactions)
        {
            running += t.Type == "Credit" ? t.Amount : -t.Amount;
            result.Add(new BankTransactionDto(t.Id, t.BankAccountId, t.TransactionDate, t.Type, t.Amount, t.Category, t.Description, running, t.CreatedAt));
        }
        return result;
    }
}
