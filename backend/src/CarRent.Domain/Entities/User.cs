namespace CarRent.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid RoleId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = default!;
    public Department? Department { get; set; }
    public Branch? Branch { get; set; }
}
