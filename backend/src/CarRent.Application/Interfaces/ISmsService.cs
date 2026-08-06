namespace CarRent.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}
