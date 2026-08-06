namespace CarRent.Domain.Entities;

public class MaintenanceExpense
{
    public Guid Id { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? MaintenanceId { get; set; }
    public Guid? VendorId { get; set; }
    public string Category { get; set; } = "Other";
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
