using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Services.Reports;

namespace CarRent.Infrastructure.Services.Finance;

public class LedgerService : ILedgerService
{
    private readonly IFinanceTransactionService _transactionService;

    public LedgerService(IFinanceTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<List<LedgerAccountDto>> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var transactions = await _transactionService.GetTransactionsAsync(effectiveFrom, effectiveTo, cancellationToken);

        return transactions
            .GroupBy(t => t.Category)
            .Select(g => new LedgerAccountDto(
                g.Key,
                g.Where(t => t.Type == "Income").Sum(t => t.Amount),
                g.Where(t => t.Type == "Expense").Sum(t => t.Amount),
                g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount),
                g.OrderByDescending(t => t.Date).ToList()))
            .OrderByDescending(a => Math.Abs(a.Net))
            .ToList();
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var accounts = await GetAsync(from, to, cancellationToken);

        var kpis = new List<ReportKpiDto>
        {
            new("Accounts", accounts.Count, "number", null),
            new("Net Total", accounts.Sum(a => a.Net), "currency", null)
        };

        var sections = accounts.Select(a => new ReportExportSection(
            a.Account,
            new[] { "Date", "Description", "Type", "Amount" },
            a.Entries.Select(e => new[] { e.Date.ToString("d"), e.Description, e.Type, e.Amount.ToString("N2") }).ToList())).ToList();

        var model = new ReportExportModel("Ledger", from, to, kpis, sections);

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
