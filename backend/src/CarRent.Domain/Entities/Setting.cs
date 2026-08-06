namespace CarRent.Domain.Entities;

public class Setting
{
    public Guid Id { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
