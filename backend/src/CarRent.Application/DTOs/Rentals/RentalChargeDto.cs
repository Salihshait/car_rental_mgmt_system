namespace CarRent.Application.DTOs.Rentals;

public class RentalChargeDto
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRentalChargeRequest
{
    public string ChargeType { get; set; } = "Other";
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}
