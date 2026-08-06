namespace CarRent.Application.DTOs.Maintenance;

public class VendorPerformanceDto
{
    public Guid VendorId { get; set; }
    public string? VendorName { get; set; }
    public int JobCount { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal AverageCost { get; set; }
}
