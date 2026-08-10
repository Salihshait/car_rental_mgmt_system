namespace CarRent.Application.DTOs.Ai;

public record FraudAlertDto(
    Guid Id,
    Guid? BookingId,
    Guid? PaymentId,
    Guid CustomerId,
    string? CustomerName,
    int RiskScore,
    string Reasons,
    string Status,
    DateTime CreatedAt,
    Guid? ReviewedBy,
    DateTime? ReviewedAt);

public record ReviewFraudAlertRequest(string Status);
