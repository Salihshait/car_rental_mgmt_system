namespace CarRent.Domain.Entities;

public class BookingExtension
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public DateTime PreviousEndDate { get; set; }
    public DateTime NewEndDate { get; set; }
    public decimal AdditionalAmount { get; set; }
    public string Status { get; set; } = "Approved";
    public string? Reason { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
}
