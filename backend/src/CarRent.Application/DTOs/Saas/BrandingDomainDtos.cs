namespace CarRent.Application.DTOs.Saas;

public record TenantBrandingDto(Guid TenantId, string? LogoUrl, string? PrimaryColor, string? SecondaryColor, string? CompanyDisplayName, string? FaviconUrl, DateTime? UpdatedAt);

public record UpsertTenantBrandingRequest(string? LogoUrl, string? PrimaryColor, string? SecondaryColor, string? CompanyDisplayName, string? FaviconUrl);

public record TenantDomainDto(Guid Id, Guid TenantId, string Domain, string Status, DateTime CreatedAt);

public record CreateTenantDomainRequest(string Domain);
