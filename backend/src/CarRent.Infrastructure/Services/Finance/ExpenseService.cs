using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Services.Reports;

namespace CarRent.Infrastructure.Services.Finance;

public class ExpenseService : IExpenseService
{
    private readonly IFinanceTransactionService _transactionService;

    public ExpenseService(IFinanceTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<ExpenseSummaryDto> GetSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var expenses = (await _transactionService.GetTransactionsAsync(effectiveFrom, effectiveTo, cancellationToken))
            .Where(t => t.Type == "Expense")
            .ToList();

        var total = expenses.Sum(t => t.Amount);
        var count = expenses.Count;
        var avg = count == 0 ? 0 : Math.Round(total / count, 2);

        var periodLength = effectiveTo - effectiveFrom;
        var previousFrom = effectiveFrom - periodLength;
        var previousExpenses = (await _transactionService.GetTransactionsAsync(previousFrom, effectiveFrom, cancellationToken))
            .Where(t => t.Type == "Expense")
            .Sum(t => t.Amount);
        decimal? growthPercent = previousExpenses == 0 ? null : Math.Round((total - previousExpenses) / previousExpenses * 100, 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Expenses", total, "currency", growthPercent),
            new("Transactions", count, "number", null),
            new("Avg Transaction", avg, "currency", null)
        };

        var trend = expenses
            .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(t => t.Amount)))
            .ToList();

        var byCategory = expenses
            .GroupBy(t => t.Category)
            .Select(g => new ChartPointDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        return new ExpenseSummaryDto(kpis, trend, byCategory, expenses.OrderByDescending(t => t.Date).ToList());
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(from, to, cancellationToken);

        var model = new ReportExportModel(
            "Expense Report",
            from,
            to,
            summary.Kpis,
            new List<ReportExportSection>
            {
                new("Expense Trend", new[] { "Period", "Expenses" }, summary.Trend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("By Category", new[] { "Category", "Amount" }, summary.ByCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Transactions", new[] { "Date", "Category", "Description", "Amount" },
                    summary.Transactions.Select(t => new[] { t.Date.ToString("d"), t.Category, t.Description, t.Amount.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
