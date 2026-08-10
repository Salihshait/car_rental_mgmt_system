using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRent.Infrastructure.Services.Reports;

public static class ReportPdfBuilder
{
    public static byte[] Build(ReportExportModel model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text(model.Title).FontSize(18).Bold();
                    column.Item().Text($"Period: {model.From?.ToString("d") ?? "-"} to {model.To?.ToString("d") ?? "-"}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        foreach (var kpi in model.Kpis)
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                            {
                                c.Item().Text(kpi.Label).FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(FormatValue(kpi.Value, kpi.Format)).FontSize(14).Bold();
                            });
                        }
                    });

                    foreach (var section in model.Sections)
                    {
                        column.Item().Text(section.Title).FontSize(12).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in section.Headers)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            table.Header(header =>
                            {
                                foreach (var h in section.Headers)
                                {
                                    header.Cell().Text(h).Bold().FontSize(9);
                                }
                            });

                            foreach (var rowData in section.Rows.Take(200))
                            {
                                foreach (var cellValue in rowData)
                                {
                                    table.Cell().Text(cellValue).FontSize(8);
                                }
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatValue(decimal value, string format) => format switch
    {
        "currency" => value.ToString("N2"),
        "percent" => $"{value:N1}%",
        _ => value.ToString("N0")
    };
}
