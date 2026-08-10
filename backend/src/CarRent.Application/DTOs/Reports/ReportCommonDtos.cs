namespace CarRent.Application.DTOs.Reports;

public record ReportKpiDto(string Label, decimal Value, string Format, decimal? TrendPercent);

public record ChartPointDto(string Key, decimal Value);
