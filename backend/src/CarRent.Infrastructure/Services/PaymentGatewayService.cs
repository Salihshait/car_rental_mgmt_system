using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;

namespace CarRent.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly Dictionary<string, IPaymentGatewayProvider> _providers;

    public PaymentGatewayService(IEnumerable<IPaymentGatewayProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public Task<GatewayOrderResult> CreateOrderAsync(string gateway, decimal amount, string currency, string receipt, CancellationToken cancellationToken = default) =>
        Resolve(gateway).CreateOrderAsync(amount, currency, receipt, cancellationToken);

    public Task<GatewayVerificationResult> VerifyPaymentAsync(string gateway, string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default) =>
        Resolve(gateway).VerifyPaymentAsync(orderReference, paymentReference, signature, cancellationToken);

    public Task<GatewayRefundResult> RefundAsync(string gateway, string paymentReference, decimal amount, CancellationToken cancellationToken = default) =>
        Resolve(gateway).RefundAsync(paymentReference, amount, cancellationToken);

    private IPaymentGatewayProvider Resolve(string gateway) =>
        _providers.TryGetValue(gateway, out var provider)
            ? provider
            : throw new InvalidOperationException($"'{gateway}' is not a supported payment gateway.");
}
