using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class UploadDriverDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public IFormFile? File { get; set; }
}

public class VerifyDriverDocumentRequest
{
    public string VerificationStatus { get; set; } = string.Empty;
}

[ApiController]
[Route("api/drivers/{driverId:guid}/documents")]
[Authorize]
public class DriverDocumentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";
    private const string DocumentsBucket = "driver-documents";

    private readonly IDriverDocumentService _documentService;
    private readonly IDriverService _driverService;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public DriverDocumentsController(IDriverDocumentService documentService, IDriverService driverService, ISupabaseAdminClient supabaseAdminClient)
    {
        _documentService = documentService;
        _driverService = driverService;
        _supabaseAdminClient = supabaseAdminClient;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private bool IsAdmin => User.IsInRole("Super Admin") || User.IsInRole("Company Admin") || User.IsInRole("Branch Manager");

    private async Task<IActionResult?> EnsureSelfOrAdminAsync(Guid driverId, CancellationToken cancellationToken)
    {
        if (IsAdmin)
        {
            return null;
        }

        var driver = await _driverService.GetByUserIdAsync(CurrentUserId, cancellationToken);
        return driver is not null && driver.Id == driverId ? null : Forbid();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid driverId, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureSelfOrAdminAsync(driverId, cancellationToken);
        if (forbidden is not null) return forbidden;

        return Ok(await _documentService.GetByDriverAsync(driverId, cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(Guid driverId, [FromForm] UploadDriverDocumentRequest request, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureSelfOrAdminAsync(driverId, cancellationToken);
        if (forbidden is not null) return forbidden;

        try
        {
            string? storagePath = null;

            if (request.File is { Length: > 0 })
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);
                var extension = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = "pdf";
                }

                var path = $"{driverId}/{Guid.NewGuid()}.{extension}";
                storagePath = await _supabaseAdminClient.UploadFileAsync(DocumentsBucket, path, stream.ToArray(), request.File.ContentType, cancellationToken);
            }

            var created = await _documentService.CreateAsync(driverId, request.DocumentType, request.DocumentNumber, request.ExpiryDate, storagePath, CurrentUserId, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/verify")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Verify(Guid driverId, Guid id, [FromBody] VerifyDriverDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _documentService.VerifyAsync(driverId, id, request.VerificationStatus, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid driverId, Guid id, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureSelfOrAdminAsync(driverId, cancellationToken);
        if (forbidden is not null) return forbidden;

        try
        {
            await _documentService.DeleteAsync(driverId, id, CurrentUserId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
