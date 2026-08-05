namespace CarRent.Domain.Entities;

public class VehicleDocument
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? StoragePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
