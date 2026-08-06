using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// UPI here means "customer paid via a UPI app and staff/the customer enters the UTR reference" -
/// not a live UPI collect-request integration - so there's no order/verify round-trip either.
/// Used by RecordManualPaymentAsync, not the order flow.
/// </summary>
public class UpiGatewayProvider : IPaymentGatewayProvider
{
    public string Name => "UPI";

    public Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayOrderResult { OrderId = $"upi_{Guid.NewGuid():N}", Amount = amount, Currency = currency, Status = "Created" });

    public Task<GatewayVerificationResult> VerifyPaymentAsync(string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayVerificationResult { IsVerified = true });

    public Task<GatewayRefundResult> RefundAsync(string paymentReference, decimal amount, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayRefundResult { RefundReference = $"upi-refund_{Guid.NewGuid():N}", Status = "Processed" });
}
