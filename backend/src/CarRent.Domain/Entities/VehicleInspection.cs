namespace CarRent.Domain.Entities;

public class VehicleInspection
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public string InspectionType { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    public DateTime? NextDueDate { get; set; }
    public string Result { get; set; } = "Pass";
    public string? InspectorName { get; set; }
    public Guid? VendorId { get; set; }
    public string? Notes { get; set; }
    public string? CertificateUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
