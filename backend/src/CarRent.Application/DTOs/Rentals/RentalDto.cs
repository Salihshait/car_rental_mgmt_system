namespace CarRent.Application.DTOs.Rentals;

public class RentalDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public string Status { get; set; } = string.Empty;

    public DateTime PickupAt { get; set; }
    public int PickupOdometerReading { get; set; }
    public decimal PickupFuelLevelPercent { get; set; }
    public string? PickupConditionNotes { get; set; }
    public Guid PickupStaffUserId { get; set; }

    public DateTime? AgreementSignedAt { get; set; }
    public string? AgreementSignatureUrl { get; set; }
    public string? AgreementPdfUrl { get; set; }

    public DateTime? ReturnAt { get; set; }
    public int? ReturnOdometerReading { get; set; }
    public decimal? ReturnFuelLevelPercent { get; set; }
    public string? ReturnConditionNotes { get; set; }
    public Guid? ReturnStaffUserId { get; set; }

    public decimal SecurityDepositAmount { get; set; }
    public string SecurityDepositStatus { get; set; } = string.Empty;
    public decimal? SecurityDepositRefundAmount { get; set; }

    public decimal LateFeeAmount { get; set; }
    public int LateHours { get; set; }
    public Guid? FinalInvoiceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
