using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.DTOs.Finance;

public record GstDetailRowDto(string InvoiceNumber, DateTime IssueDate, string? BranchName, decimal TaxableValue, decimal Cgst, decimal Sgst, decimal Igst, decimal TotalTax, decimal TotalAmount);

public record GstSummaryDto(List<ReportKpiDto> Kpis, List<ChartPointDto> ByMonth, List<ChartPointDto> ByBranch, List<GstDetailRowDto> DetailRows);
