namespace CarRent.Application.DTOs.Vehicles;

public class FleetDashboardDto
{
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public double UtilizationPercent { get; set; }
    public decimal Revenue { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}
