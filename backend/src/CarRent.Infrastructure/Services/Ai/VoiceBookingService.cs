using System.Text.Json;
using CarRent.Application.DTOs.Ai;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services.Ai;

public class VoiceBookingService : IVoiceBookingService
{
    private static readonly string[] VehicleCategoryKeywords = { "sedan", "suv", "hatchback", "van", "luxury" };

    private readonly CarRentDbContext _context;
    private readonly IVoiceTranscriptionProvider _provider;

    public VoiceBookingService(CarRentDbContext context, IVoiceTranscriptionProvider provider)
    {
        _context = context;
        _provider = provider;
    }

    public async Task<VoiceBookingRequestDto> SubmitAsync(Guid customerId, byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        var transcript = await _provider.TranscribeAsync(audioBytes, cancellationToken);
        var intent = ParseIntent(transcript);

        var request = new VoiceBookingRequest
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TranscribedText = transcript,
            ParsedIntentJson = JsonSerializer.Serialize(intent),
            Status = "Transcribed"
        };

        await _context.VoiceBookingRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new VoiceBookingRequestDto(request.Id, request.CustomerId, request.TranscribedText, intent, request.Status, request.CreatedAt);
    }

    public async Task<IEnumerable<VoiceBookingRequestDto>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var requests = await _context.VoiceBookingRequests.AsNoTracking()
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => new VoiceBookingRequestDto(
            r.Id, r.CustomerId, r.TranscribedText,
            JsonSerializer.Deserialize<Dictionary<string, string>>(r.ParsedIntentJson) ?? new Dictionary<string, string>(),
            r.Status, r.CreatedAt));
    }

    private static Dictionary<string, string> ParseIntent(string transcript)
    {
        var lower = transcript.ToLowerInvariant();
        var intent = new Dictionary<string, string>();

        var category = VehicleCategoryKeywords.FirstOrDefault(lower.Contains);
        if (category is not null)
        {
            intent["VehicleCategory"] = category;
        }

        if (lower.Contains("next weekend"))
        {
            var daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)DateTime.UtcNow.DayOfWeek + 7) % 7;
            var nextSaturday = DateTime.UtcNow.AddDays(daysUntilSaturday == 0 ? 7 : daysUntilSaturday);
            intent["SuggestedStartDate"] = nextSaturday.ToString("yyyy-MM-dd");
        }
        else if (lower.Contains("tomorrow"))
        {
            intent["SuggestedStartDate"] = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        }

        intent["RawTranscript"] = transcript;
        return intent;
    }
}
