using CarRent.Application.DTOs.Finance;
using CarRent.Application.DTOs.Reports;
using CarRent.Application.Interfaces;
using CarRent.Infrastructure.Services.Reports;

namespace CarRent.Infrastructure.Services.Finance;

public class ProfitLossService : IProfitLossService
{
    private readonly IFinanceTransactionService _transactionService;

    public ProfitLossService(IFinanceTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<ProfitLossDto> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddMonths(-6);
        var effectiveTo = to ?? DateTime.UtcNow;

        var transactions = await _transactionService.GetTransactionsAsync(effectiveFrom, effectiveTo, cancellationToken);
        var income = transactions.Where(t => t.Type == "Income").ToList();
        var expense = transactions.Where(t => t.Type == "Expense").ToList();

        var totalIncome = income.Sum(t => t.Amount);
        var totalExpense = expense.Sum(t => t.Amount);
        var netProfit = totalIncome - totalExpense;
        var margin = totalIncome == 0 ? 0 : Math.Round(netProfit / totalIncome * 100, 1);

        var kpis = new List<ReportKpiDto>
        {
            new("Total Income", totalIncome, "currency", null),
            new("Total Expenses", totalExpense, "currency", null),
            new("Net Profit", netProfit, "currency", null),
            new("Net Margin", margin, "percent", null)
        };

        var incomeByCategory = income
            .GroupBy(t => t.Category)
            .Select(g => new ChartPointDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        var expenseByCategory = expense
            .GroupBy(t => t.Category)
            .Select(g => new ChartPointDto(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(p => p.Value)
            .ToList();

        var monthlyTrend = transactions
            .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new ChartPointDto(g.Key.ToString("MMM yyyy"), g.Sum(t => t.Type == "Income" ? t.Amount : -t.Amount)))
            .ToList();

        return new ProfitLossDto(kpis, incomeByCategory, expenseByCategory, monthlyTrend);
    }

    public async Task<byte[]> ExportAsync(string format, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var pl = await GetAsync(from, to, cancellationToken);

        var model = new ReportExportModel(
            "Profit & Loss Statement",
            from,
            to,
            pl.Kpis,
            new List<ReportExportSection>
            {
                new("Income By Category", new[] { "Category", "Amount" }, pl.IncomeByCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Expenses By Category", new[] { "Category", "Amount" }, pl.ExpenseByCategory.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList()),
                new("Monthly Net", new[] { "Period", "Net" }, pl.MonthlyTrend.Select(p => new[] { p.Key, p.Value.ToString("N2") }).ToList())
            });

        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
            ? ReportPdfBuilder.Build(model)
            : ReportWorkbookBuilder.Build(model);
    }
}
