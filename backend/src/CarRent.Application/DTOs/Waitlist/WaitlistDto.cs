namespace CarRent.Application.DTOs.Waitlist;

public class WaitlistDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public Guid? VehicleCategoryId { get; set; }
    public string? VehicleCategoryName { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime DesiredStartDate { get; set; }
    public DateTime DesiredEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
}
