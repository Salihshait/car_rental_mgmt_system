namespace CarRent.Domain.Entities;

public class RentalCharge
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string ChargeType { get; set; } = "Other";
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
