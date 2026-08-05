using System.Security.Claims;
using CarRent.Application.DTOs.Users;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

public class UpdateStatusRequest
{
    public bool IsActive { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private const string AdminRoles = "Super Admin,Company Admin";

    private readonly IUserService _userService;
    private readonly ISupabaseAdminClient _supabaseAdminClient;

    public UsersController(IUserService userService, ISupabaseAdminClient supabaseAdminClient)
    {
        _userService = userService;
        _supabaseAdminClient = supabaseAdminClient;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpPost("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileRequest request, CancellationToken cancellationToken)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "No email claim present on the authenticated token." });
        }

        try
        {
            var created = await _userService.CompleteProfileAsync(CurrentUserId, email, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(CurrentUserId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _userService.UpdateProfileAsync(CurrentUserId, request, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("me/logout-all")]
    public async Task<IActionResult> LogoutAllForSelf(CancellationToken cancellationToken)
    {
        await _supabaseAdminClient.LogoutAllSessionsAsync(CurrentUserId, cancellationToken);
        return NoContent();
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> UploadMyAvatar(IFormFile file, CancellationToken cancellationToken) =>
        UploadAvatarInternal(CurrentUserId, file, cancellationToken);

    [HttpGet]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _userService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _userService.UpdateAsync(id, request, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _userService.UpdateStatusAsync(id, request.IsActive, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/avatar")]
    [Authorize(Roles = AdminRoles)]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> UploadAvatar(Guid id, IFormFile file, CancellationToken cancellationToken) =>
        UploadAvatarInternal(id, file, cancellationToken);

    [HttpPost("{id:guid}/logout-all")]
    [Authorize(Roles = AdminRoles)]
    public async Task<IActionResult> LogoutAll(Guid id, CancellationToken cancellationToken)
    {
        await _supabaseAdminClient.LogoutAllSessionsAsync(id, cancellationToken);
        return NoContent();
    }

    private async Task<IActionResult> UploadAvatarInternal(Guid userId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);
        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = "jpg";
        }

        try
        {
            var avatarUrl = await _supabaseAdminClient.UploadFileAsync("avatars", $"{userId}.{extension}", stream.ToArray(), file.ContentType, cancellationToken);
            var updated = await _userService.SetAvatarAsync(userId, avatarUrl, cancellationToken);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
