namespace CarRent.Application.DTOs.Customers;

public class CustomerDocumentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? StoragePath { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
