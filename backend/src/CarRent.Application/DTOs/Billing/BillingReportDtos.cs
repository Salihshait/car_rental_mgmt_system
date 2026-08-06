namespace CarRent.Application.DTOs.Billing;

public class RevenueSummaryDto
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal CgstCollected { get; set; }
    public decimal SgstCollected { get; set; }
    public decimal IgstCollected { get; set; }
    public decimal TotalRefundsIssued { get; set; }
    public decimal TotalSecurityDepositsHeld { get; set; }
    public int InvoiceCount { get; set; }
}

public class OutstandingInvoiceDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public int DaysOutstanding { get; set; }
}

public class PaymentMethodBreakdownDto
{
    public string Gateway { get; set; } = string.Empty;
    public int PaymentCount { get; set; }
    public decimal TotalAmount { get; set; }
}
