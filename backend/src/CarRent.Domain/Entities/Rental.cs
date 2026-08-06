namespace CarRent.Domain.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Status { get; set; } = "Active";

    public DateTime PickupAt { get; set; } = DateTime.UtcNow;
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
    public Guid? SecurityDepositPaymentId { get; set; }
    public string SecurityDepositStatus { get; set; } = "Held";
    public decimal? SecurityDepositRefundAmount { get; set; }

    public decimal LateFeeAmount { get; set; }
    public int LateHours { get; set; }
    public Guid? FinalInvoiceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
