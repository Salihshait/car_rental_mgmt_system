namespace CarRent.Domain.Entities;

public class SparePart
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal UnitCost { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public Guid? PreferredVendorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
