namespace CarRent.Application.DTOs.Drivers;

public class DriverRatingDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Guid RatedBy { get; set; }
    public string? RatedByName { get; set; }
    public int Score { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDriverRatingRequest
{
    public Guid DriverId { get; set; }
    public int Score { get; set; }
    public string Category { get; set; } = "Overall";
    public string? Comment { get; set; }
}
