namespace CarRent.Application.DTOs.Maintenance;

public class MaintenanceCostSummaryDto
{
    public decimal TotalCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal ExpensesCost { get; set; }
    public Dictionary<string, decimal> CostByCategory { get; set; } = new();
    public List<VehicleCostDto> CostByVehicle { get; set; } = new();
}

public class VehicleCostDto
{
    public Guid VehicleId { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal TotalCost { get; set; }
}
