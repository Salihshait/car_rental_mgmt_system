namespace CarRent.Application.DTOs.Vehicles;

public class VehicleImportRowResultDto
{
    public int RowNumber { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? RegistrationNumber { get; set; }
}

public class VehicleImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<VehicleImportRowResultDto> Rows { get; set; } = new();
}
