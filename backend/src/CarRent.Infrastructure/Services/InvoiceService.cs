using CarRent.Application.DTOs.Invoices;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly CarRentDbContext _context;
    private readonly IInvoicePdfService _pdfService;

    public InvoiceService(CarRentDbContext context, IInvoicePdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return await MapAsync(invoices, cancellationToken);
    }

    public async Task<InvoiceSummaryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        var mapped = await MapAsync(new List<Invoice> { invoice }, cancellationToken);
        return mapped.First();
    }

    public async Task<InvoiceSummaryDto> GenerateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("The selected booking does not exist.");

        if (await _context.Invoices.AnyAsync(i => i.BookingId == request.BookingId, cancellationToken))
        {
            throw new InvalidOperationException("An invoice already exists for this booking.");
        }

        var manualDiscount = request.ManualDiscountAmount ?? 0;
        if (manualDiscount < 0)
        {
            throw new InvalidOperationException("Manual discount cannot be negative.");
        }

        var totalAmount = booking.TotalAmount - manualDiscount;
        if (totalAmount < 0)
        {
            throw new InvalidOperationException("Manual discount cannot exceed the booking total.");
        }

        var invoiceCount = await _context.Invoices.CountAsync(cancellationToken);

        var rental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.BookingId == booking.Id, cancellationToken);
        var rentalCharges = rental is null
            ? new List<RentalCharge>()
            : await _context.RentalCharges.AsNoTracking().Where(c => c.RentalId == rental.Id).ToListAsync(cancellationToken);
        var extraChargesTotal = rentalCharges.Sum(c => c.Amount);

        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == booking.VehicleId, cancellationToken);
        var branch = vehicle is null ? null : await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == vehicle.BranchId, cancellationToken);
        var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == booking.CustomerId, cancellationToken);

        var subtotal = booking.SubtotalAmount + extraChargesTotal;
        var discount = booking.DiscountAmount + manualDiscount;
        var gstSplit = GstSplitHelper.Split(booking.TaxAmount, branch?.State, customer?.BillingState);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{invoiceCount + 1:D4}",
            IssueDate = DateTime.UtcNow,
            SubtotalAmount = subtotal,
            DiscountAmount = discount,
            CgstAmount = gstSplit.Cgst,
            SgstAmount = gstSplit.Sgst,
            IgstAmount = gstSplit.Igst,
            TaxAmount = booking.TaxAmount,
            TotalAmount = totalAmount,
            BranchGstin = branch?.Gstin,
            CustomerGstin = customer?.Gstin,
            PlaceOfSupply = branch?.State,
            Status = "Unpaid"
        };

        var lineItems = new List<InvoiceLineItem>
        {
            new() { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Description = "Rental charge", Amount = booking.SubtotalAmount, ItemType = "Rental" }
        };

        lineItems.AddRange(rentalCharges.Select(c => new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Description = string.IsNullOrWhiteSpace(c.Description) ? c.ChargeType : $"{c.ChargeType}: {c.Description}",
            Amount = c.Amount,
            ItemType = c.ChargeType switch { "Damage" => "Damage", "Late" => "LateFee", _ => "Other" }
        }));

        if (discount > 0)
        {
            var description = manualDiscount > 0
                ? request.ManualDiscountReason ?? "Discount"
                : "Coupon discount";
            lineItems.Add(new InvoiceLineItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Description = description, Amount = -discount, ItemType = "Discount" });
        }

        if (gstSplit.Cgst > 0)
        {
            lineItems.Add(new InvoiceLineItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Description = "CGST", Amount = gstSplit.Cgst, ItemType = "Tax" });
        }

        if (gstSplit.Sgst > 0)
        {
            lineItems.Add(new InvoiceLineItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Description = "SGST", Amount = gstSplit.Sgst, ItemType = "Tax" });
        }

        if (gstSplit.Igst > 0)
        {
            lineItems.Add(new InvoiceLineItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Description = "IGST", Amount = gstSplit.Igst, ItemType = "Tax" });
        }

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.InvoiceLineItems.AddRangeAsync(lineItems, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(invoice, lineItems);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        var lineItems = await _context.InvoiceLineItems.AsNoTracking().Where(li => li.InvoiceId == invoiceId).ToListAsync(cancellationToken);

        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == invoice.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == booking.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");

        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == booking.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == vehicle.BranchId, cancellationToken);

        return _pdfService.Generate(invoice, lineItems, booking, vehicle, customer, branch);
    }

    private async Task<List<InvoiceSummaryDto>> MapAsync(List<Invoice> invoices, CancellationToken cancellationToken)
    {
        var invoiceIds = invoices.Select(i => i.Id).ToList();

        var lineItems = await _context.InvoiceLineItems
            .AsNoTracking()
            .Where(li => invoiceIds.Contains(li.InvoiceId))
            .ToListAsync(cancellationToken);

        return invoices
            .Select(invoice => ToDto(invoice, lineItems.Where(li => li.InvoiceId == invoice.Id).ToList()))
            .ToList();
    }

    private static InvoiceSummaryDto ToDto(Invoice invoice, List<InvoiceLineItem> lineItems) => new()
    {
        Id = invoice.Id,
        BookingId = invoice.BookingId,
        InvoiceNumber = invoice.InvoiceNumber,
        IssueDate = invoice.IssueDate,
        SubtotalAmount = invoice.SubtotalAmount,
        DiscountAmount = invoice.DiscountAmount,
        CgstAmount = invoice.CgstAmount,
        SgstAmount = invoice.SgstAmount,
        IgstAmount = invoice.IgstAmount,
        TaxAmount = invoice.TaxAmount,
        TotalAmount = invoice.TotalAmount,
        AmountPaid = invoice.AmountPaid,
        DueDate = invoice.DueDate,
        BranchGstin = invoice.BranchGstin,
        CustomerGstin = invoice.CustomerGstin,
        PlaceOfSupply = invoice.PlaceOfSupply,
        Status = invoice.Status,
        LineItems = lineItems.Select(li => new InvoiceLineItemDto { Description = li.Description, Amount = li.Amount, ItemType = li.ItemType }).ToList()
    };
}
