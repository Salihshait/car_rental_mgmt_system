namespace CarRent.Application.DTOs.Rentals;

public class RentalPhotoDto
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string StorageUrl { get; set; } = string.Empty;
    public Guid? RentalDamageId { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
