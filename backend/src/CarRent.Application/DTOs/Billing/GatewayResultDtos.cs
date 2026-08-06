namespace CarRent.Application.DTOs.Billing;

public class GatewayOrderResult
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = "Created";
}

public class GatewayVerificationResult
{
    public bool IsVerified { get; set; }
    public string? Message { get; set; }
}

public class GatewayRefundResult
{
    public string RefundReference { get; set; } = string.Empty;
    public string Status { get; set; } = "Processed";
}
