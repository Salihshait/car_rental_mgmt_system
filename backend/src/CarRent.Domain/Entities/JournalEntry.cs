namespace CarRent.Domain.Entities;

public class JournalEntry
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string EntryType { get; set; } = "Income";
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
