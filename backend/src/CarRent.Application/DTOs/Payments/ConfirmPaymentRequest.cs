namespace CarRent.Application.DTOs.Payments;

public class ConfirmPaymentRequest
{
    public string GatewayPaymentReference { get; set; } = string.Empty;
    public string? Signature { get; set; }
}
