namespace CarRent.Domain.Entities;

public class BankTransaction
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = "Credit";
    public decimal Amount { get; set; }
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
