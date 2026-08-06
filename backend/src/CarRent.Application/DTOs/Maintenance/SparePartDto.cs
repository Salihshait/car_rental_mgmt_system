namespace CarRent.Application.DTOs.Maintenance;

public class SparePartDto
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal UnitCost { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public Guid? PreferredVendorId { get; set; }
    public string? PreferredVendorName { get; set; }
    public bool IsLowStock { get; set; }
}

public class SaveSparePartRequest
{
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal UnitCost { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public Guid? PreferredVendorId { get; set; }
}

public class AdjustStockRequest
{
    public int QuantityChange { get; set; }
    public string? Reason { get; set; }
}
