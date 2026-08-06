using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// No live Razorpay account is configured yet. Mimics the Razorpay Orders API request/response
/// shape closely enough that swapping this for a real HttpClient-based implementation later is a
/// drop-in replacement - only this class changes, callers are unaffected.
/// </summary>
public class RazorpayGatewayProvider : IPaymentGatewayProvider
{
    private readonly ILogger<RazorpayGatewayProvider> _logger;

    public RazorpayGatewayProvider(ILogger<RazorpayGatewayProvider> logger)
    {
        _logger = logger;
    }

    public string Name => "Razorpay";

    public Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default)
    {
        var orderId = $"order_{Guid.NewGuid():N}"[..20];
        _logger.LogInformation("[RazorpayStub] Created order {OrderId} for {Amount} {Currency} (receipt {Receipt})", orderId, amount, currency, receipt);
        return Task.FromResult(new GatewayOrderResult { OrderId = orderId, Amount = amount, Currency = currency, Status = "Created" });
    }

    public Task<GatewayVerificationResult> VerifyPaymentAsync(string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[RazorpayStub] Verifying payment {PaymentReference} against order {OrderReference}", paymentReference, orderReference);
        return Task.FromResult(new GatewayVerificationResult { IsVerified = true });
    }

    public Task<GatewayRefundResult> RefundAsync(string paymentReference, decimal amount, CancellationToken cancellationToken = default)
    {
        var refundId = $"rfnd_{Guid.NewGuid():N}"[..18];
        _logger.LogInformation("[RazorpayStub] Refunded {Amount} against payment {PaymentReference} as {RefundId}", amount, paymentReference, refundId);
        return Task.FromResult(new GatewayRefundResult { RefundReference = refundId, Status = "Processed" });
    }
}
