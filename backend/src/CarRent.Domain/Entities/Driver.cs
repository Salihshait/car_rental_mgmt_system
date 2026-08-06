namespace CarRent.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string KycStatus { get; set; } = "Pending";
    public decimal? Rating { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public decimal? BaseSalary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
