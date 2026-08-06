using CarRent.Application.DTOs.Billing;
using CarRent.Application.Interfaces;

namespace CarRent.Infrastructure.Services;

/// <summary>
/// Cash has no third-party settlement - staff record the transaction as already completed,
/// so there's no order/verify round-trip. Used by RecordManualPaymentAsync, not the order flow.
/// </summary>
public class CashGatewayProvider : IPaymentGatewayProvider
{
    public string Name => "Cash";

    public Task<GatewayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayOrderResult { OrderId = $"cash_{Guid.NewGuid():N}", Amount = amount, Currency = currency, Status = "Created" });

    public Task<GatewayVerificationResult> VerifyPaymentAsync(string orderReference, string paymentReference, string? signature, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayVerificationResult { IsVerified = true });

    public Task<GatewayRefundResult> RefundAsync(string paymentReference, decimal amount, CancellationToken cancellationToken = default) =>
        Task.FromResult(new GatewayRefundResult { RefundReference = $"cash-refund_{Guid.NewGuid():N}", Status = "Processed" });
}
