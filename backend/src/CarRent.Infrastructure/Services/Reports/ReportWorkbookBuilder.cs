using ClosedXML.Excel;

namespace CarRent.Infrastructure.Services.Reports;

public static class ReportWorkbookBuilder
{
    public static byte[] Build(ReportExportModel model)
    {
        using var workbook = new XLWorkbook();

        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = model.Title;
        summary.Cell(1, 1).Style.Font.Bold = true;
        summary.Cell(1, 1).Style.Font.FontSize = 14;

        summary.Cell(2, 1).Value = "Period";
        summary.Cell(2, 2).Value = $"{model.From?.ToString("d") ?? "-"} to {model.To?.ToString("d") ?? "-"}";

        summary.Cell(4, 1).Value = "KPI";
        summary.Cell(4, 2).Value = "Value";
        summary.Range(4, 1, 4, 2).Style.Font.Bold = true;

        var row = 5;
        foreach (var kpi in model.Kpis)
        {
            summary.Cell(row, 1).Value = kpi.Label;
            summary.Cell(row, 2).Value = FormatValue(kpi.Value, kpi.Format);
            row++;
        }
        summary.Columns().AdjustToContents();

        foreach (var section in model.Sections)
        {
            var sheet = workbook.Worksheets.Add(SanitizeSheetName(section.Title));

            for (var c = 0; c < section.Headers.Length; c++)
            {
                sheet.Cell(1, c + 1).Value = section.Headers[c];
                sheet.Cell(1, c + 1).Style.Font.Bold = true;
            }

            for (var r = 0; r < section.Rows.Count; r++)
            {
                var rowData = section.Rows[r];
                for (var c = 0; c < rowData.Length; c++)
                {
                    sheet.Cell(r + 2, c + 1).Value = rowData[c];
                }
            }

            sheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string FormatValue(decimal value, string format) => format switch
    {
        "currency" => value.ToString("N2"),
        "percent" => $"{value:N1}%",
        _ => value.ToString("N0")
    };

    private static string SanitizeSheetName(string name)
    {
        var invalid = new[] { '\\', '/', '?', '*', '[', ']', ':' };
        var sanitized = new string(name.Where(c => !invalid.Contains(c)).ToArray());
        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }
}
