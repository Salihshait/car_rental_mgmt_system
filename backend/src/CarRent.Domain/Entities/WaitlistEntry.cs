namespace CarRent.Domain.Entities;

public class WaitlistEntry
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime DesiredStartDate { get; set; }
    public DateTime DesiredEndDate { get; set; }
    public string Status { get; set; } = "Waiting";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NotifiedAt { get; set; }
}
