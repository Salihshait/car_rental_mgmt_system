using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.DTOs.Saas;

public record RecordUsageMetricRequest(string MetricKey, decimal MetricValue);

public record PlatformOverviewDto(List<ReportKpiDto> Kpis, List<ChartPointDto> TenantGrowthTrend, List<ChartPointDto> MrrTrend);

public record TenantUsageDto(List<ReportKpiDto> Kpis, List<ChartPointDto> Trend);
