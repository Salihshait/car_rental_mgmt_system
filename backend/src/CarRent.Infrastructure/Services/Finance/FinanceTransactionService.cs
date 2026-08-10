using CarRent.Application.DTOs.Finance;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class FinanceTransactionService : IFinanceTransactionService
{
    private readonly CarRentDbContext _context;

    public FinanceTransactionService(CarRentDbContext context)
    {
        _context = context;
    }

    public async Task<List<FinanceTransactionDto>> GetTransactionsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var transactions = new List<FinanceTransactionDto>();

        var paymentsQuery = _context.Payments.AsNoTracking().Where(p => p.Status == "Verified");
        if (from.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaidAt >= from);
        if (to.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaidAt <= to);
        var payments = await paymentsQuery.ToListAsync(cancellationToken);
        transactions.AddRange(payments.Select(p => new FinanceTransactionDto(
            p.PaidAt ?? p.CreatedAt, "Income", "Booking Revenue", $"Payment ({p.Purpose})", p.Amount, "Payment", p.Id)));

        var expensesQuery = _context.MaintenanceExpenses.AsNoTracking().AsQueryable();
        if (from.HasValue) expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= from);
        if (to.HasValue) expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= to);
        var expenses = await expensesQuery.ToListAsync(cancellationToken);
        transactions.AddRange(expenses.Select(e => new FinanceTransactionDto(
            e.ExpenseDate, "Expense", $"Maintenance: {e.Category}", e.Description ?? e.Category, e.Amount, "MaintenanceExpense", e.Id)));

        var salaryQuery = _context.DriverSalaryPayments.AsNoTracking().Where(s => s.Status == "Paid");
        if (from.HasValue) salaryQuery = salaryQuery.Where(s => s.PaidAt >= from);
        if (to.HasValue) salaryQuery = salaryQuery.Where(s => s.PaidAt <= to);
        var salaries = await salaryQuery.ToListAsync(cancellationToken);
        transactions.AddRange(salaries.Select(s => new FinanceTransactionDto(
            s.PaidAt ?? s.CreatedAt, "Expense", "Driver Salary", $"Salary {s.PeriodStart:d} - {s.PeriodEnd:d}", s.NetAmount, "DriverSalaryPayment", s.Id)));

        var refundsQuery = _context.Refunds.AsNoTracking().Where(r => r.Status == "Processed");
        if (from.HasValue) refundsQuery = refundsQuery.Where(r => r.RequestedAt >= from);
        if (to.HasValue) refundsQuery = refundsQuery.Where(r => r.RequestedAt <= to);
        var refunds = await refundsQuery.ToListAsync(cancellationToken);
        transactions.AddRange(refunds.Select(r => new FinanceTransactionDto(
            r.RequestedAt, "Expense", "Refunds", r.Reason ?? "Refund", r.Amount, "Refund", r.Id)));

        var journalQuery = _context.JournalEntries.AsNoTracking().AsQueryable();
        if (from.HasValue) journalQuery = journalQuery.Where(j => j.EntryDate >= from);
        if (to.HasValue) journalQuery = journalQuery.Where(j => j.EntryDate <= to);
        var journalEntries = await journalQuery.ToListAsync(cancellationToken);
        transactions.AddRange(journalEntries.Select(j => new FinanceTransactionDto(
            j.EntryDate, j.EntryType, j.Category, j.Description, j.Amount, "JournalEntry", j.Id)));

        return transactions.OrderBy(t => t.Date).ToList();
    }
}
