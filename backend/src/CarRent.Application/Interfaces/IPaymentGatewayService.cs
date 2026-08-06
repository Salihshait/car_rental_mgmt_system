using CarRent.Application.DTOs.Billing;

namespace CarRent.Application.Interfaces;

/// <summary>Resolves the right IPaymentGatewayProvider by name and dispatches to it.</summary>
public interface IPaymentGatewayService
{
    Task<GatewayOrderResult> CreateOrderAsync(string gateway, decimal amount, string currency, string receipt, CancellationToken cancellationToken = default);
    Task<GatewayVerificationResult> VerifyPaymentAsync(string gateway, string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default);
    Task<GatewayRefundResult> RefundAsync(string gateway, string paymentReference, decimal amount, CancellationToken cancellationToken = default);
}
