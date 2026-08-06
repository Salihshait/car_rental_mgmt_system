namespace CarRent.Application.DTOs.Maintenance;

public class WorkshopDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaveWorkshopRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
