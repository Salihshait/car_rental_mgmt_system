using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRent.Infrastructure.Services;

public class InvoicePdfService : IInvoicePdfService
{
    public byte[] Generate(Invoice invoice, IReadOnlyList<InvoiceLineItem> lineItems, Booking booking, Vehicle vehicle, User customer, Branch? branch)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Tax Invoice").FontSize(18).Bold();
                    column.Item().Text($"Invoice Number: {invoice.InvoiceNumber}").FontSize(9);
                    column.Item().Text($"Issue Date: {invoice.IssueDate:d}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Billed To").Bold();
                            c.Item().Text($"{customer.FirstName} {customer.LastName}");
                            c.Item().Text(customer.Email);
                            if (!string.IsNullOrWhiteSpace(invoice.CustomerGstin))
                            {
                                c.Item().Text($"GSTIN: {invoice.CustomerGstin}");
                            }
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Supplier").Bold();
                            c.Item().Text(branch?.Name ?? "-");
                            if (!string.IsNullOrWhiteSpace(invoice.BranchGstin))
                            {
                                c.Item().Text($"GSTIN: {invoice.BranchGstin}");
                            }
                            if (!string.IsNullOrWhiteSpace(invoice.PlaceOfSupply))
                            {
                                c.Item().Text($"Place of Supply: {invoice.PlaceOfSupply}");
                            }
                        });
                    });

                    column.Item().Text("Vehicle").Bold();
                    column.Item().Text($"{vehicle.RegistrationNumber} - {booking.StartDate:d} to {booking.EndDate:d}");

                    column.Item().PaddingTop(10).Text("Line Items").Bold();
                    foreach (var lineItem in lineItems)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(lineItem.Description);
                            row.ConstantItem(100).AlignRight().Text(lineItem.Amount.ToString("C"));
                        });
                    }

                    column.Item().PaddingTop(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                    AddSummaryRow(column, "Subtotal", invoice.SubtotalAmount);
                    if (invoice.DiscountAmount > 0)
                    {
                        AddSummaryRow(column, "Discount", -invoice.DiscountAmount);
                    }
                    if (invoice.CgstAmount > 0)
                    {
                        AddSummaryRow(column, "CGST", invoice.CgstAmount);
                    }
                    if (invoice.SgstAmount > 0)
                    {
                        AddSummaryRow(column, "SGST", invoice.SgstAmount);
                    }
                    if (invoice.IgstAmount > 0)
                    {
                        AddSummaryRow(column, "IGST", invoice.IgstAmount);
                    }
                    AddSummaryRow(column, "Total", invoice.TotalAmount, bold: true);
                    AddSummaryRow(column, "Amount Paid", invoice.AmountPaid);
                    AddSummaryRow(column, "Balance Due", invoice.TotalAmount - invoice.AmountPaid, bold: true);

                    column.Item().PaddingTop(10).Text($"Payment Status: {invoice.Status}").Bold();
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span($"{DateTime.UtcNow:u}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AddSummaryRow(QuestPDF.Fluent.ColumnDescriptor column, string label, decimal amount, bool bold = false)
    {
        column.Item().Row(row =>
        {
            if (bold)
            {
                row.RelativeItem().AlignRight().Text(label).Bold();
                row.ConstantItem(100).AlignRight().Text(amount.ToString("C")).Bold();
            }
            else
            {
                row.RelativeItem().AlignRight().Text(label);
                row.ConstantItem(100).AlignRight().Text(amount.ToString("C"));
            }
        });
    }
}
