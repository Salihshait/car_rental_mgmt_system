namespace CarRent.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }
}
