using CarRent.Application.Interfaces;

namespace CarRent.Infrastructure.Services.Ai;

/// <summary>
/// No real LLM configured yet. Uses keyword-matched FAQ-style rules so the chatbot is actually
/// useful for common questions rather than a flat placeholder. Swap for a real OpenAI/Anthropic
/// call by registering a different IChatbotProvider in Program.cs.
/// </summary>
public class RuleBasedChatbotProvider : IChatbotProvider
{
    private static readonly (string[] Keywords, string Reply)[] Rules =
    {
        (new[] { "hello", "hi", "hey" }, "Hello! I can help with bookings, pricing, and cancellations. What do you need?"),
        (new[] { "book", "booking", "rent" }, "To book a vehicle, browse our fleet, pick your dates, and confirm — I can also start a voice booking if you'd rather describe what you need."),
        (new[] { "price", "cost", "rate" }, "Pricing depends on the vehicle, dates, and demand. Check a vehicle's listing for its current daily rate."),
        (new[] { "cancel", "refund" }, "You can cancel an upcoming booking from your account's booking history. Refunds follow our cancellation policy."),
        (new[] { "maintenance", "damage" }, "For vehicle condition or maintenance concerns, our staff can run a damage inspection or check the maintenance schedule for you."),
        (new[] { "thanks", "thank you" }, "You're welcome! Anything else I can help with?"),
    };

    public Task<string> GenerateReplyAsync(IReadOnlyList<string> history, string message, CancellationToken cancellationToken = default)
    {
        var lower = message.ToLowerInvariant();
        foreach (var (keywords, reply) in Rules)
        {
            if (keywords.Any(lower.Contains))
            {
                return Task.FromResult(reply);
            }
        }

        return Task.FromResult("I'm a simple rule-based assistant for now, so I might not have understood that. Try asking about booking, pricing, or cancellations, or contact support for anything more specific.");
    }
}
