using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services.Reports;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class BalanceSheetService : IBalanceSheetService
{
    private readonly CarRentDbContext _context;
    private readonly IFinanceTransactionService _transactionService;

    public BalanceSheetService(CarRentDbContext context, IFinanceTransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    public async Task<BalanceSheetDto> GetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        var effectiveAsOf = asOfDate ?? DateTime.UtcNow;

        var openingBalances = await _context.BankAccounts.AsNoTracking()
            .Where(a => a.IsActive)
            .SumAsync(a => (decimal?)a.OpeningBalance, cancellationToken) ?? 0;

        var accountIds = await _context.BankAccounts.AsNoTracking().Where(a => a.IsActive).Select(a => a.Id).ToListAsync(cancellationToken);
        var bankTransactions = await _context.BankTransactions.AsNoTracking()
            .Where(t => accountIds.Contains(t.BankAccountId) && t.TransactionDate <= effectiveAsOf)
            .ToListAsync(cancellationToken);
        var bankNet = bankTransactions.Sum(t => t.Type == "Credit" ? t.Amount : -t.Amount);
        var bankBalance = openingBalances + bankNet;

        var outstandingInvoices = await _context.Invoices.AsNoTracking()
            .Where(i => (i.Status == "Unpaid" || i.Status == "PartiallyPaid") && i.IssueDate <= effectiveAsOf)
            .SumAsync(i => (decimal?)(i.TotalAmount - i.AmountPaid), cancellationToken) ?? 0;

        var securityDepositsHeld = await _context.Rentals.AsNoTracking()
            .Where(r => r.SecurityDepositStatus == "Held")
            .SumAsync(r => (decimal?)r.SecurityDepositAmount, cancellationToken) ?? 0;

        var refundsPayable = await _context.Refunds.AsNoTracking()
            .Where(r => r.Status != "Processed" && r.RequestedAt <= effectiveAsOf)
            .SumAsync(r => (decimal?)r.Amount, cancellationToken) ?? 0;

        var transactions = await _transactionService.GetTransactionsAsync(null, effectiveAsOf, cancellationToken);
        var retainedEarnings = transactions.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount);

        var assets = new List<BalanceSheetLineDto>
        {
            new("Cash & Bank Balances", bankBalance),
            new("Accounts Receivable", outstandingInvoices),
            new("Security Deposits Held (cash)", securityDepositsHeld)
        };

        var liabilities = new List<BalanceSheetLineDto>
        {
            new("Security Deposits Payable", securityDepositsHeld),
            new("Refunds Payable", refundsPayable)
        };

        var equity = new List<BalanceSheetLineDto>
        {
            new("Retained Earnings", retainedEarnings)
        };

        var totalAssets = assets.Sum(l => l.Amount);
        var totalLiabilities = liabilities.Sum(l => l.Amount);
        var totalEquity = equity.Sum(l => l.Amount);
        var difference = totalAssets - (totalLiabilities + totalEquity);

        return new BalanceSheetDto(effectiveAsOf, assets, liabilities, equity, totalAssets, totalLiabilities, totalEquity, difference);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        var balanceSheet = await GetAsync(asOfDate, cancellationToken);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Assets", balanceSheet.TotalAssets, "currency", null),
            new("Total Liabilities", balanceSheet.TotalLiabilities, "currency", null),
            new("Total Equity", balanceSheet.TotalEquity, "currency", null),
            new("Difference", balanceSheet.Difference, "currency", null)
        };

        var model = new ReportExportModel(
            "Balance Sheet",
            null,
            balanceSheet.AsOfDate,
            kpis,
            new List<ReportExportSection>
            {
                new("Assets", new[] { "Label", "Amount" }, balanceSheet.Assets.Select(l => new[] { l.Label, l.Amount.ToString("N2") }).ToList()),
                new("Liabilities", new[] { "Label", "Amount" }, balanceSheet.Liabilities.Select(l => new[] { l.Label, l.Amount.ToString("N2") }).ToList()),
                new("Equity", new[] { "Label", "Amount" }, balanceSheet.Equity.Select(l => new[] { l.Label, l.Amount.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
