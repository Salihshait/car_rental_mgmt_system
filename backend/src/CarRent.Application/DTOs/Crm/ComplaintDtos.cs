namespace CarRent.Application.DTOs.Crm;

public record ComplaintDto(
    Guid Id,
    Guid CustomerId,
    string? CustomerName,
    Guid? BookingId,
    Guid? VehicleId,
    string Subject,
    string Description,
    string Severity,
    string Status,
    string? Resolution,
    Guid? AssignedToUserId,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public record CreateComplaintRequest(string Subject, string Description, string Severity, Guid? BookingId, Guid? VehicleId);

public record ResolveComplaintRequest(string Status, string? Resolution);
