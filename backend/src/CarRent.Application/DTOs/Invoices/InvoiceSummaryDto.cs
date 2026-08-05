namespace CarRent.Application.DTOs.Invoices;

public class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
}
