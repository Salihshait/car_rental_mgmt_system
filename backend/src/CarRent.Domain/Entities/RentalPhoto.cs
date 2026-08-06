namespace CarRent.Domain.Entities;

public class RentalPhoto
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Stage { get; set; } = "Pickup";
    public string Category { get; set; } = "Other";
    public string StorageUrl { get; set; } = string.Empty;
    public Guid? RentalDamageId { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
