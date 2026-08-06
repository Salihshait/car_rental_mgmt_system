namespace CarRent.Application.DTOs.Rentals;

public class RentalFilter
{
    public string? Status { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
