namespace CarRent.Application.DTOs.Maintenance;

public class SparePartsConsumptionReportDto
{
    public List<SparePartConsumptionEntryDto> Consumption { get; set; } = new();
    public List<SparePartDto> LowStockParts { get; set; } = new();
}

public class SparePartConsumptionEntryDto
{
    public Guid SparePartId { get; set; }
    public string? PartNumber { get; set; }
    public string? Name { get; set; }
    public int QuantityUsed { get; set; }
    public decimal TotalCost { get; set; }
    public int CurrentStock { get; set; }
}
