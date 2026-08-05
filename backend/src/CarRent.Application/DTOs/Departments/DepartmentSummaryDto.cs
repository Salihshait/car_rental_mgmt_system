namespace CarRent.Application.DTOs.Departments;

public class DepartmentSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
}
