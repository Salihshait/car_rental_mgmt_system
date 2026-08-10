namespace CarRent.Domain.Entities;

public class DocumentOcrResult
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string ExtractedFieldsJson { get; set; } = "{}";
    public decimal ConfidenceScore { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
