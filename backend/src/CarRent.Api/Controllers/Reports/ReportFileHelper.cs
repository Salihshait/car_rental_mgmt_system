using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

internal static class ReportFileHelper
{
    public static IActionResult ToFileResult(ControllerBase controller, string fileNamePrefix, string format, byte[] bytes)
    {
        var isPdf = string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase);
        var contentType = isPdf ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var extension = isPdf ? "pdf" : "xlsx";
        return controller.File(bytes, contentType, $"{fileNamePrefix}.{extension}");
    }
}
