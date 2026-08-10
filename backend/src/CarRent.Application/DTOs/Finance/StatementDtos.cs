using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.DTOs.Finance;

public record ProfitLossDto(List<ReportKpiDto> Kpis, List<ChartPointDto> IncomeByCategory, List<ChartPointDto> ExpenseByCategory, List<ChartPointDto> MonthlyTrend);

public record BalanceSheetLineDto(string Label, decimal Amount);

public record BalanceSheetDto(
    DateTime AsOfDate,
    List<BalanceSheetLineDto> Assets,
    List<BalanceSheetLineDto> Liabilities,
    List<BalanceSheetLineDto> Equity,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal TotalEquity,
    decimal Difference);
