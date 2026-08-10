namespace CarRent.Application.Interfaces;

public interface IWhatsAppService
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}
