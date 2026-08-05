using CarRent.Application.DTOs.Invoices;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly CarRentDbContext _context;

    public InvoiceService(CarRentDbContext context)
    {
        _context = context;
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
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);
        if (booking is null)
        {
            throw new InvalidOperationException("The selected booking does not exist.");
        }

        var invoiceExists = await _context.Invoices.AnyAsync(i => i.BookingId == request.BookingId, cancellationToken);
        if (invoiceExists)
        {
            throw new InvalidOperationException("An invoice already exists for this booking.");
        }

        var invoiceCount = await _context.Invoices.CountAsync(cancellationToken);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{invoiceCount + 1:D4}",
            IssueDate = DateTime.UtcNow,
            TotalAmount = booking.TotalAmount,
            Status = "Unpaid"
        };

        var lineItem = new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Description = "Rental charge",
            Amount = booking.TotalAmount
        };

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.InvoiceLineItems.AddAsync(lineItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvoiceSummaryDto
        {
            Id = invoice.Id,
            BookingId = invoice.BookingId,
            InvoiceNumber = invoice.InvoiceNumber,
            IssueDate = invoice.IssueDate,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            LineItems = new List<InvoiceLineItemDto>
            {
                new() { Description = lineItem.Description, Amount = lineItem.Amount }
            }
        };
    }

    private async Task<List<InvoiceSummaryDto>> MapAsync(List<Invoice> invoices, CancellationToken cancellationToken)
    {
        var invoiceIds = invoices.Select(i => i.Id).ToList();

        var lineItems = await _context.InvoiceLineItems
            .AsNoTracking()
            .Where(li => invoiceIds.Contains(li.InvoiceId))
            .ToListAsync(cancellationToken);

        return invoices
            .Select(invoice => new InvoiceSummaryDto
            {
                Id = invoice.Id,
                BookingId = invoice.BookingId,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                LineItems = lineItems
                    .Where(li => li.InvoiceId == invoice.Id)
                    .Select(li => new InvoiceLineItemDto { Description = li.Description, Amount = li.Amount })
                    .ToList()
            })
            .ToList();
    }
}
