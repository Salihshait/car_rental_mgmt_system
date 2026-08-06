namespace CarRent.Application.DTOs.Fleet;

public class FleetDashboardSummaryDto
{
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public double UtilizationPercent { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public int MaintenanceDueCount { get; set; }
    public int MaintenanceOverdueCount { get; set; }
    public int ActiveTripCount { get; set; }
    public int VehiclesReportingGpsCount { get; set; }
    public int ActiveDriverAssignmentCount { get; set; }
    public int InTransitTransferCount { get; set; }
    public decimal FuelCostThisMonth { get; set; }
}
