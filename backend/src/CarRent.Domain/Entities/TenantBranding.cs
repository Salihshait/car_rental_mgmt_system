namespace CarRent.Domain.Entities;

public class TenantBranding
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? CompanyDisplayName { get; set; }
    public string? FaviconUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
