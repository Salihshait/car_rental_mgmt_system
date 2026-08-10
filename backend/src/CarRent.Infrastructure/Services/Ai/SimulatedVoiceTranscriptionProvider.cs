using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services.Ai;

/// <summary>
/// No real speech-to-text provider configured yet. Returns a fixed, clearly-labeled simulated
/// transcript so the voice-booking pipeline can be exercised end-to-end. Swap for a real
/// speech-to-text call by registering a different IVoiceTranscriptionProvider in Program.cs.
/// </summary>
public class SimulatedVoiceTranscriptionProvider : IVoiceTranscriptionProvider
{
    private readonly ILogger<SimulatedVoiceTranscriptionProvider> _logger;

    public SimulatedVoiceTranscriptionProvider(ILogger<SimulatedVoiceTranscriptionProvider> logger)
    {
        _logger = logger;
    }

    public Task<string> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[VoiceTranscriptionStub] Simulated transcription for a {Size}-byte audio clip", audioBytes.Length);
        return Task.FromResult("I need a car for next weekend, something like an SUV.");
    }
}
