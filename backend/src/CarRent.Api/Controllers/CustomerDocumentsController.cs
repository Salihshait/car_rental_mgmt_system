using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class UploadCustomerDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public IFormFile? File { get; set; }
}

public class VerifyCustomerDocumentRequest
{
    public string VerificationStatus { get; set; } = string.Empty;
}

[ApiController]
[Route("api/customers/{customerId:guid}/documents")]
[Authorize]
public class CustomerDocumentsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";

    private readonly ICustomerDocumentService _documentService;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public CustomerDocumentsController(ICustomerDocumentService documentService, ISupabaseAdminClient supabaseAdminClient)
    {
        _documentService = documentService;
        _supabaseAdminClient = supabaseAdminClient;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private bool IsAdmin => User.IsInRole("Super Admin") || User.IsInRole("Company Admin") || User.IsInRole("Branch Manager");

    private IActionResult? EnsureSelfOrAdmin(Guid customerId) =>
        IsAdmin || customerId == CurrentUserId ? null : Forbid();

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid customerId, CancellationToken cancellationToken)
    {
        var forbidden = EnsureSelfOrAdmin(customerId);
        if (forbidden is not null) return forbidden;

        return Ok(await _documentService.GetByCustomerAsync(customerId, cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(Guid customerId, [FromForm] UploadCustomerDocumentRequest request, CancellationToken cancellationToken)
    {
        var forbidden = EnsureSelfOrAdmin(customerId);
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

                var path = $"{customerId}/{Guid.NewGuid()}.{extension}";
                storagePath = await _supabaseAdminClient.UploadFileAsync("customer-documents", path, stream.ToArray(), request.File.ContentType, cancellationToken);
            }

            var created = await _documentService.CreateAsync(customerId, request.DocumentType, request.DocumentNumber, request.ExpiryDate, storagePath, CurrentUserId, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/verify")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Verify(Guid customerId, Guid id, [FromBody] VerifyCustomerDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _documentService.VerifyAsync(customerId, id, request.VerificationStatus, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid customerId, Guid id, CancellationToken cancellationToken)
    {
        var forbidden = EnsureSelfOrAdmin(customerId);
        if (forbidden is not null) return forbidden;

        try
        {
            await _documentService.DeleteAsync(customerId, id, CurrentUserId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
