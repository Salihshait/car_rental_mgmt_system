namespace CarRent.Application.DTOs.Invoices;

public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue => TotalAmount - AmountPaid;
    public DateTime? DueDate { get; set; }
    public string? BranchGstin { get; set; }
    public string? CustomerGstin { get; set; }
    public string? PlaceOfSupply { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
}
