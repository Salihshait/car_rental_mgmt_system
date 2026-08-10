using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class ExtractDocumentRequest
{
    public IFormFile? Image { get; set; }
}

[ApiController]
[Route("api/ai/ocr")]
[Authorize]
public class DocumentOcrController : ControllerBase
{
    private readonly IDocumentOcrService _documentOcrService;

    public DocumentOcrController(IDocumentOcrService documentOcrService)
    {
        _documentOcrService = documentOcrService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("driving-license")]
    public Task<IActionResult> ExtractDrivingLicense([FromForm] ExtractDocumentRequest request, CancellationToken cancellationToken) =>
        ExtractAsync("DrivingLicense", request, cancellationToken);

    [HttpPost("rc-book")]
    public Task<IActionResult> ExtractRcBook([FromForm] ExtractDocumentRequest request, CancellationToken cancellationToken) =>
        ExtractAsync("RcBook", request, cancellationToken);

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string? documentType, CancellationToken cancellationToken) =>
        Ok(await _documentOcrService.GetHistoryAsync(documentType, cancellationToken));

    private async Task<IActionResult> ExtractAsync(string documentType, ExtractDocumentRequest request, CancellationToken cancellationToken)
    {
        if (request.Image is null || request.Image.Length == 0)
        {
            return BadRequest(new { message = "An image file is required." });
        }

        using var stream = new MemoryStream();
        await request.Image.CopyToAsync(stream, cancellationToken);

        var result = await _documentOcrService.ExtractAsync(documentType, stream.ToArray(), CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
