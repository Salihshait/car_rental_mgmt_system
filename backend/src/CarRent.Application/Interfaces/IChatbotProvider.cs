namespace CarRent.Application.Interfaces;

public interface IChatbotProvider
{
    Task<string> GenerateReplyAsync(IReadOnlyList<string> history, string message, CancellationToken cancellationToken = default);
}
