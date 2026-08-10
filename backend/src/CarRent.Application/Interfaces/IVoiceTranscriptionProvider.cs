namespace CarRent.Application.Interfaces;

public interface IVoiceTranscriptionProvider
{
    Task<string> TranscribeAsync(byte[] audioBytes, CancellationToken cancellationToken = default);
}
