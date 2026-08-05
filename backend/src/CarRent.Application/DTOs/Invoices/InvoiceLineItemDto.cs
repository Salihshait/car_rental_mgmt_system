namespace CarRent.Application.DTOs.Invoices;

public class InvoiceLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
