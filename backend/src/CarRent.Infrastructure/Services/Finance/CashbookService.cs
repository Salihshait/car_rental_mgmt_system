using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services.Reports;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Finance;

public class CashbookService : ICashbookService
{
    private readonly IFinanceTransactionService _transactionService;
    private readonly CarRentDbContext _context;

    public CashbookService(IFinanceTransactionService transactionService, CarRentDbContext context)
    {
        _transactionService = transactionService;
        _context = context;
    }

    public async Task<List<CashbookEntryDto>> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var openingBalance = await _context.BankAccounts.AsNoTracking()
            .Where(a => a.IsActive)
            .SumAsync(a => (decimal?)a.OpeningBalance, cancellationToken) ?? 0;

        var transactions = await _transactionService.GetTransactionsAsync(effectiveFrom, effectiveTo, cancellationToken);

        var runningBalance = openingBalance;
        var entries = new List<CashbookEntryDto>();
        foreach (var t in transactions)
        {
            runningBalance += t.Type == "Income" ? t.Amount : -t.Amount;
            entries.Add(new CashbookEntryDto(t.Date, t.Description, t.Category, t.Type, t.Amount, runningBalance));
        }

        return entries;
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var entries = await GetAsync(from, to, cancellationToken);

        var openingBalance = entries.Count > 0 ? entries[0].RunningBalance - (entries[0].Type == "Income" ? entries[0].Amount : -entries[0].Amount) : 0;
        var closingBalance = entries.Count > 0 ? entries[^1].RunningBalance : openingBalance;
        var totalCredits = entries.Where(e => e.Type == "Income").Sum(e => e.Amount);
        var totalDebits = entries.Where(e => e.Type == "Expense").Sum(e => e.Amount);

        var kpis = new List<ReportKpiDto>
        {
            new("Opening Balance", openingBalance, "currency", null),
            new("Closing Balance", closingBalance, "currency", null),
            new("Total Credits", totalCredits, "currency", null),
            new("Total Debits", totalDebits, "currency", null)
        };

        var model = new ReportExportModel(
            "Cashbook",
            from,
            to,
            kpis,
            new List<ReportExportSection>
            {
                new("Entries", new[] { "Date", "Description", "Category", "Type", "Amount", "Balance" },
                    entries.Select(e => new[] { e.Date.ToString("d"), e.Description, e.Category, e.Type, e.Amount.ToString("N2"), e.RunningBalance.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
