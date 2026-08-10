namespace CarRent.Application.DTOs.Reports;

public record FinanceDetailRowDto(string InvoiceNumber, DateTime IssueDate, decimal TotalAmount, decimal AmountDue, int DaysOutstanding);

public record FinanceDashboardDto(
    List<ReportKpiDto> Kpis,
    List<ChartPointDto> RevenueTrend,
    List<ChartPointDto> ExpensesTrend,
    List<ChartPointDto> PaymentMethods,
    List<FinanceDetailRowDto> DetailRows);
