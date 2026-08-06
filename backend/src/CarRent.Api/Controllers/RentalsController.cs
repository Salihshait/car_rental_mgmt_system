using System.Security.Claims;
using CarRent.Application.DTOs.Rentals;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class AddRentalPhotoRequest
{
    public string Stage { get; set; } = "Pickup";
    public string Category { get; set; } = "Other";
    public Guid? RentalDamageId { get; set; }
    public IFormFile? File { get; set; }
}

[ApiController]
[Route("api/rentals")]
[Authorize]
public class RentalsController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin,Branch Manager";
    private const string DocumentsBucket = "rental-documents";

    private readonly IRentalService _rentalService;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public RentalsController(IRentalService rentalService, ISupabaseAdminClient supabaseAdminClient)
    {
        _rentalService = rentalService;
        _supabaseAdminClient = supabaseAdminClient;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private bool IsAdmin => User.IsInRole("Super Admin") || User.IsInRole("Company Admin") || User.IsInRole("Branch Manager");

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var filter = new RentalFilter { Status = status, BranchId = branchId, DateFrom = dateFrom, DateTo = dateTo };
        return Ok(await _rentalService.GetAllAsync(filter, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var rental = await _rentalService.GetByIdAsync(id, cancellationToken);
        if (rental is null) return NotFound();
        if (!IsAdmin && rental.CustomerId != CurrentUserId) return Forbid();
        return Ok(rental);
    }

    [HttpGet("by-booking/{bookingId:guid}")]
    public async Task<IActionResult> GetByBookingId(Guid bookingId, CancellationToken cancellationToken)
    {
        var rental = await _rentalService.GetByBookingIdAsync(bookingId, cancellationToken);
        if (rental is null) return NotFound();
        if (!IsAdmin && rental.CustomerId != CurrentUserId) return Forbid();
        return Ok(rental);
    }

    [HttpPost("pickup")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Pickup([FromBody] CreatePickupRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _rentalService.PickupAsync(request, CurrentUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/signature")]
    [Authorize(Roles = AdminRoles)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadSignature(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No signature image uploaded." });
        }

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            var signatureBytes = stream.ToArray();

            var signaturePath = $"{id}/signature-{Guid.NewGuid()}.png";
            var signatureUrl = await _supabaseAdminClient.UploadFileAsync(DocumentsBucket, signaturePath, signatureBytes, file.ContentType, cancellationToken);

            var pdfBytes = await _rentalService.GenerateAgreementPdfAsync(id, signatureBytes, cancellationToken);
            var pdfPath = $"{id}/agreement-{Guid.NewGuid()}.pdf";
            var pdfUrl = await _supabaseAdminClient.UploadFileAsync(DocumentsBucket, pdfPath, pdfBytes, "application/pdf", cancellationToken);

            var updated = await _rentalService.CompleteAgreementAsync(id, signatureUrl, pdfUrl, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/damages")]
    public async Task<IActionResult> GetDamages(Guid id, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureAccessAsync(id, cancellationToken);
        if (forbidden is not null) return forbidden;

        return Ok(await _rentalService.GetDamagesAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/damages")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> AddDamage(Guid id, [FromBody] CreateRentalDamageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _rentalService.AddDamageAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/photos")]
    public async Task<IActionResult> GetPhotos(Guid id, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureAccessAsync(id, cancellationToken);
        if (forbidden is not null) return forbidden;

        return Ok(await _rentalService.GetPhotosAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = AdminRoles)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPhoto(Guid id, [FromForm] AddRentalPhotoRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { message = "No photo uploaded." });
        }

        try
        {
            using var stream = new MemoryStream();
            await request.File.CopyToAsync(stream, cancellationToken);
            var extension = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = "jpg";
            }

            var path = $"{id}/photos/{Guid.NewGuid()}.{extension}";
            var url = await _supabaseAdminClient.UploadFileAsync(DocumentsBucket, path, stream.ToArray(), request.File.ContentType, cancellationToken);

            var photo = await _rentalService.AddPhotoAsync(id, request.Stage, request.Category, url, request.RentalDamageId, CurrentUserId, cancellationToken);
            return Ok(photo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/charges")]
    public async Task<IActionResult> GetCharges(Guid id, CancellationToken cancellationToken)
    {
        var forbidden = await EnsureAccessAsync(id, cancellationToken);
        if (forbidden is not null) return forbidden;

        return Ok(await _rentalService.GetChargesAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/charges")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> AddCharge(Guid id, [FromBody] CreateRentalChargeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _rentalService.AddChargeAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/return")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Return(Guid id, [FromBody] CreateReturnRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _rentalService.ReturnAsync(id, request, CurrentUserId, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("reports/summary")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetReportSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken) =>
        Ok(await _rentalService.GetReportSummaryAsync(from, to, branchId, cancellationToken));

    private async Task<IActionResult?> EnsureAccessAsync(Guid rentalId, CancellationToken cancellationToken)
    {
        if (IsAdmin)
        {
            return null;
        }

        var rental = await _rentalService.GetByIdAsync(rentalId, cancellationToken);
        if (rental is null)
        {
            return NotFound();
        }

        return rental.CustomerId == CurrentUserId ? null : Forbid();
    }
}
