namespace CarRent.Application.DTOs.Users;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public DateTime CreatedAt { get; set; }
}
