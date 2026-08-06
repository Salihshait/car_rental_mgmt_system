using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No live Stripe account is configured yet. Mimics the Stripe PaymentIntents API shape closely
/// enough that swapping this for a real HttpClient-based implementation later is a drop-in
/// replacement - only this class changes, callers are unaffected.
/// </summary>
public class StripeGatewayProvider : IPaymentGatewayProvider
{
    private readonly ILogger<StripeGatewayProvider> _logger;

    public StripeGatewayProvider(ILogger<StripeGatewayProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "Stripe";

    public Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default)
    {
        var intentId = $"pi_{Guid.NewGuid():N}"[..24];
        _logger.LogInformation("[StripeStub] Created PaymentIntent {IntentId} for {Amount} {Currency} (receipt {Receipt})", intentId, amount, currency, receipt);
        return Task.FromResult(new GatewayOrderResult { OrderId = intentId, Amount = amount, Currency = currency, Status = "Created" });
    }

    public Task<GatewayVerificationResult> VerifyPaymentAsync(string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[StripeStub] Confirming PaymentIntent {OrderReference} with charge {PaymentReference}", orderReference, paymentReference);
        return Task.FromResult(new GatewayVerificationResult { IsVerified = true });
    }

    public Task<GatewayRefundResult> RefundAsync(string paymentReference, decimal amount, CancellationToken cancellationToken = default)
    {
        var refundId = $"re_{Guid.NewGuid():N}"[..20];
        _logger.LogInformation("[StripeStub] Refunded {Amount} against charge {PaymentReference} as {RefundId}", amount, paymentReference, refundId);
        return Task.FromResult(new GatewayRefundResult { RefundReference = refundId, Status = "Processed" });
    }
}
