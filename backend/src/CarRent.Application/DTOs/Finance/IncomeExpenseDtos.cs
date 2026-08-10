using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.DTOs.Finance;

public record IncomeSummaryDto(List<ReportKpiDto> Kpis, List<ChartPointDto> Trend, List<ChartPointDto> ByCategory, List<FinanceTransactionDto> Transactions);

public record ExpenseSummaryDto(List<ReportKpiDto> Kpis, List<ChartPointDto> Trend, List<ChartPointDto> ByCategory, List<FinanceTransactionDto> Transactions);
