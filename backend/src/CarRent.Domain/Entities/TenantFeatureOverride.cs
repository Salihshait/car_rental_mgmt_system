namespace CarRent.Domain.Entities;

public class TenantFeatureOverride
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
