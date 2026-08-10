using CarRent.Application.DTOs.Reports;

namespace CarRent.Infrastructure.Services.Reports;

public record ReportExportSection(string Title, string[] Headers, List<string[]> Rows);

public record ReportExportModel(string Title, DateTime? From, DateTime? To, List<ReportKpiDto> Kpis, List<ReportExportSection> Sections);
