using System.Security.Claims;
using CarRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers;

[ApiController]
[Route("api/ai/recommendations")]
[Authorize]
public class RecommendationEngineController : ControllerBase
{
    private readonly IRecommendationEngineService _recommendationEngineService;

    public RecommendationEngineController(IRecommendationEngineService recommendationEngineService)
    {
        _recommendationEngineService = recommendationEngineService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    [HttpGet("me")]
    public async Task<IActionResult> GetForMe(CancellationToken cancellationToken) =>
        Ok(await _recommendationEngineService.GetRecommendationsAsync(CurrentUserId, cancellationToken));
}
