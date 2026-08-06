namespace CarRent.Application.DTOs.Waitlist;

public class CreateWaitlistRequest
{
    public Guid? CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime DesiredStartDate { get; set; }
    public DateTime DesiredEndDate { get; set; }
}
