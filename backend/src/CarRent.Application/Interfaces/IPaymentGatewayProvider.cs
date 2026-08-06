using CarRent.Application.DTOs.Billing;

namespace CarRent.Application.Interfaces;

public interface IPaymentGatewayProvider
{
    string Name { get; }
    Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default);
    Task<GatewayVerificationResult> VerifyPaymentAsync(string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default);
    Task<GatewayRefundResult> RefundAsync(string paymentReference, decimal amount, CancellationToken cancellationToken = default);
}
