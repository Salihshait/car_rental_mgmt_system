namespace CarRent.Application.DTOs.Roles;

public class RoleSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
}
