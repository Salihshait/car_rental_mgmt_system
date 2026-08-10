using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Ai;

/// <summary>
/// No real computer-vision provider configured yet. Deterministically derives a plausible
/// damage report from the image bytes so damage-detection flows can be exercised end-to-end.
/// Swap for a real Google Vision/AWS Rekognition call by registering a different
/// IDamageDetectionProvider in Program.cs.
/// </summary>
public class SimulatedDamageDetectionProvider : IDamageDetectionProvider
{
    private static readonly (string Type, string[] Locations)[] DamageCatalog =
    {
        ("Scratch", new[] { "Front Bumper", "Rear Door", "Left Fender" }),
        ("Dent", new[] { "Rear Bumper", "Hood", "Right Door" }),
        ("Broken Light", new[] { "Headlight", "Taillight" }),
        ("Windshield Crack", new[] { "Windshield" }),
    };

    private readonly ILogger<SimulatedDamageDetectionProvider> _logger;

    public SimulatedDamageDetectionProvider(ILogger<SimulatedDamageDetectionProvider> logger)
    {
        _logger = logger;
    }

    public Task<DamageAnalysisResult> AnalyzeAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var seed = imageBytes.Length == 0 ? 1 : unchecked(imageBytes.Length + imageBytes.Sum(b => b));
        var random = new Random(seed);
        var damageCount = random.Next(0, 3);

        var damages = new List<DamagedAreaDto>();
        for (var i = 0; i < damageCount; i++)
        {
            var catalogEntry = DamageCatalog[random.Next(DamageCatalog.Length)];
            var location = catalogEntry.Locations[random.Next(catalogEntry.Locations.Length)];
            var confidence = Math.Round((decimal)(random.NextDouble() * 0.4 + 0.55), 2);
            damages.Add(new DamagedAreaDto(catalogEntry.Type, location, confidence));
        }

        var severity = Math.Min(100m, damages.Count * 30m);

        _logger.LogInformation("[DamageDetectionStub] Analyzed {Size}-byte image, found {Count} damage area(s)", imageBytes.Length, damages.Count);

        return Task.FromResult(new DamageAnalysisResult(damages, severity));
    }
}
