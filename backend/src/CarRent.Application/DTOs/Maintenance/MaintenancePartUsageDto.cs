namespace CarRent.Application.DTOs.Maintenance;

public class MaintenancePartUsageDto
{
    public Guid Id { get; set; }
    public Guid MaintenanceId { get; set; }
    public Guid SparePartId { get; set; }
    public string? SparePartName { get; set; }
    public string? PartNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecordPartUsageRequest
{
    public Guid SparePartId { get; set; }
    public int Quantity { get; set; }
}
