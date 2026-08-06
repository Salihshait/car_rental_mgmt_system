namespace CarRent.Application.DTOs.Fleet;

public class DriverDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string KycStatus { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
    public Guid? CurrentVehicleId { get; set; }
    public string? CurrentVehicleRegistrationNumber { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string LicenseStatus { get; set; } = "Valid";
    public decimal? BaseSalary { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDriverRequest
{
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
}

public class UpdateDriverRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string KycStatus { get; set; } = "Pending";
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
}

public class SelfUpdateDriverRequest
{
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
