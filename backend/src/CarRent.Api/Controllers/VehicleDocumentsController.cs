using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class UploadVehicleDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("api/vehicles/{vehicleId:guid}/documents")]
[Authorize]
public class VehicleDocumentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly IVehicleDocumentService _documentService;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public VehicleDocumentsController(IVehicleDocumentService documentService, ISupabaseAdminClient supabaseAdminClient)
    {
        _documentService = documentService;
        _supabaseAdminClient = supabaseAdminClient;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid vehicleId, CancellationToken cancellationToken) =>
        Ok(await _documentService.GetByVehicleAsync(vehicleId, cancellationToken));

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(Guid vehicleId, [FromForm] UploadVehicleDocumentRequest request, CancellationToken cancellationToken)
    {
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

                var path = $"{vehicleId}/{Guid.NewGuid()}.{extension}";
                storagePath = await _supabaseAdminClient.UploadFileAsync("vehicle-documents", path, stream.ToArray(), request.File.ContentType, cancellationToken);
            }

            var created = await _documentService.CreateAsync(
                vehicleId, request.DocumentType, request.DocumentNumber, request.IssuedBy, request.ExpiryDate, storagePath, CurrentUserId, cancellationToken);

            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Delete(Guid vehicleId, Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _documentService.DeleteAsync(vehicleId, id, CurrentUserId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
