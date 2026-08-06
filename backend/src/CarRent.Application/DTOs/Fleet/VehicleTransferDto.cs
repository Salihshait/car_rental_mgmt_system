namespace CarRent.Application.DTOs.Fleet;

public class VehicleTransferDto
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid FromBranchId { get; set; }
    public string? FromBranchName { get; set; }
    public Guid ToBranchId { get; set; }
    public string? ToBranchName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateTransferRequest
{
    public Guid VehicleId { get; set; }
    public Guid ToBranchId { get; set; }
    public string? Notes { get; set; }
}
