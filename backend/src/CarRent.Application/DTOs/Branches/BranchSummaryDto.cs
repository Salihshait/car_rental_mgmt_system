namespace CarRent.Application.DTOs.Branches;

public class BranchSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
