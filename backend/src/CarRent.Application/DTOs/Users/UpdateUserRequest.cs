namespace CarRent.Application.DTOs.Users;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid RoleId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
}
