using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Ai;

/// <summary>
/// No real OCR provider configured yet. Returns a plausible simulated field set per document
/// type so onboarding/document flows can be exercised end-to-end. Swap for a real Google
/// Vision/AWS Textract call by registering a different IOcrProvider in Program.cs.
/// </summary>
public class SimulatedOcrProvider : IOcrProvider
{
    private readonly ILogger<SimulatedOcrProvider> _logger;

    public SimulatedOcrProvider(ILogger<SimulatedOcrProvider> logger)
    {
        _logger = logger;
    }

    public Task<OcrExtractionResult> ExtractAsync(string documentType, byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var seed = imageBytes.Length == 0 ? 1 : unchecked(imageBytes.Length + imageBytes.Sum(b => b));
        var random = new Random(seed);

        var fields = documentType switch
        {
            "DrivingLicense" => new Dictionary<string, string>
            {
                ["Name"] = "SIMULATED NAME",
                ["LicenseNumber"] = $"DL-{random.Next(100000, 999999)}",
                ["DateOfBirth"] = new DateTime(1990, 1, 1).AddDays(random.Next(0, 3650)).ToString("yyyy-MM-dd"),
                ["ExpiryDate"] = DateTime.UtcNow.AddYears(random.Next(1, 8)).ToString("yyyy-MM-dd"),
                ["Address"] = "SIMULATED ADDRESS",
            },
            "RcBook" => new Dictionary<string, string>
            {
                ["RegistrationNumber"] = $"REG-{random.Next(1000, 9999)}",
                ["OwnerName"] = "SIMULATED OWNER",
                ["VehicleClass"] = "LMV",
                ["EngineNumber"] = $"ENG{random.Next(100000, 999999)}",
                ["ChassisNumber"] = $"CHS{random.Next(100000, 999999)}",
                ["RegistrationDate"] = DateTime.UtcNow.AddYears(-random.Next(1, 10)).ToString("yyyy-MM-dd"),
            },
            _ => new Dictionary<string, string>(),
        };

        var confidence = Math.Round((decimal)(random.NextDouble() * 0.15 + 0.8), 2);

        _logger.LogInformation("[OcrStub] Extracted {Count} simulated fields for {DocumentType} from a {Size}-byte image", fields.Count, documentType, imageBytes.Length);

        return Task.FromResult(new OcrExtractionResult(fields, confidence));
    }
}
