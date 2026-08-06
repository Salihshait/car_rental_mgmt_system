namespace CarRent.Domain.Entities;

public class MaintenancePartUsage
{
    public Guid Id { get; set; }
    public Guid MaintenanceId { get; set; }
    public Guid SparePartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
