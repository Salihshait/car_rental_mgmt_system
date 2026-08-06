namespace CarRent.Application.DTOs.Maintenance;

public class MaintenanceExpenseDto
{
    public Guid Id { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid? MaintenanceId { get; set; }
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}

public class CreateMaintenanceExpenseRequest
{
    public Guid? VehicleId { get; set; }
    public Guid? MaintenanceId { get; set; }
    public Guid? VendorId { get; set; }
    public string Category { get; set; } = "Other";
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpenseDate { get; set; }
}
