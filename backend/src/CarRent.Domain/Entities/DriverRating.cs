namespace CarRent.Domain.Entities;

public class DriverRating
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Guid RatedBy { get; set; }
    public int Score { get; set; }
    public string Category { get; set; } = "Overall";
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
