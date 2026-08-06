using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRent.Infrastructure.Services;

public class RentalAgreementPdfService : IRentalAgreementPdfService
{
    private const string TermsAndConditions =
        "The renter agrees to return the vehicle in the same condition as received, subject to normal wear and tear. " +
        "The renter is responsible for all traffic violations, tolls, and fines incurred during the rental period. " +
        "Late returns are subject to an additional late fee. Any damage found at return beyond normal wear and tear " +
        "will be charged against the security deposit, with any shortfall billed separately. The security deposit, " +
        "less any applicable deductions, will be refunded after the vehicle is inspected at return.";

    public byte[] Generate(Rental rental, Booking booking, Vehicle vehicle, User customer, byte[] signatureImageBytes)
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
                    column.Item().Text("Vehicle Rental Agreement").FontSize(18).Bold();
                    column.Item().Text($"Rental Reference: {rental.Id}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Text("Customer").Bold();
                    column.Item().Text($"{customer.FirstName} {customer.LastName}");
                    column.Item().Text(customer.Email);

                    column.Item().Text("Vehicle").Bold();
                    column.Item().Text($"{vehicle.RegistrationNumber} ({vehicle.Year} {vehicle.FuelType}, {vehicle.Transmission})");

                    column.Item().Text("Rental Period").Bold();
                    column.Item().Text($"Pickup: {booking.StartDate:f}    Scheduled Return: {booking.EndDate:f}");

                    column.Item().Text("Pricing").Bold();
                    column.Item().Text($"Subtotal: {booking.SubtotalAmount:C}    Discount: {booking.DiscountAmount:C}    Tax: {booking.TaxAmount:C}    Total: {booking.TotalAmount:C}");

                    column.Item().Text("Pickup Inspection").Bold();
                    column.Item().Text($"Odometer: {rental.PickupOdometerReading} km    Fuel Level: {rental.PickupFuelLevelPercent}%");
                    if (!string.IsNullOrWhiteSpace(rental.PickupConditionNotes))
                    {
                        column.Item().Text($"Notes: {rental.PickupConditionNotes}");
                    }

                    column.Item().Text("Terms & Conditions").Bold();
                    column.Item().Text(TermsAndConditions).FontSize(9);

                    column.Item().PaddingTop(20).Text("Customer Signature").Bold();
                    column.Item().Image(signatureImageBytes).FitWidth();
                    column.Item().Text($"Signed on {DateTime.UtcNow:f} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
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
}
