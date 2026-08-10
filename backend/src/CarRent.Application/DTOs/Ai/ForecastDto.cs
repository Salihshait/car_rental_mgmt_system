using CarRent.Application.DTOs.Reports;

namespace CarRent.Application.DTOs.Ai;

public record ForecastDto(List<ReportKpiDto> Kpis, List<ChartPointDto> Trend);
