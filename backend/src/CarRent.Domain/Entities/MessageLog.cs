namespace CarRent.Domain.Entities;

public class MessageLog
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = "Email";
    public Guid? RecipientUserId { get; set; }
    public string RecipientAddress { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
    public Guid? CampaignId { get; set; }
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = "Simulated";
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
