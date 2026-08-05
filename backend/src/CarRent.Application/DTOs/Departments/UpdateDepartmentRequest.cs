namespace CarRent.Application.DTOs.Departments;

public class UpdateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
}
