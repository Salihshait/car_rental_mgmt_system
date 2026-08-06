namespace CarRent.Application.DTOs.Waitlist;

public class WaitlistFilter
{
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? VehicleCategoryId { get; set; }
}
