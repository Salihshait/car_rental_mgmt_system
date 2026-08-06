using CarRent.Application.DTOs.Billing;
using CarRent.Application.DTOs.Payments;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CarRent.UnitTests;

public class BillingModuleTests
{
    private static CarRentDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CarRentDbContext(options);
    }

    private static PaymentGatewayService CreateGatewayService() => new(new List<CarRent.Application.Interfaces.IPaymentGatewayProvider>
    {
        new RazorpayGatewayProvider(NullLogger<RazorpayGatewayProvider>.Instance),
        new StripeGatewayProvider(NullLogger<StripeGatewayProvider>.Instance),
        new CashGatewayProvider(),
        new UpiGatewayProvider()
    });

    private static async Task<(Booking Booking, Invoice Invoice)> SeedBookingWithInvoiceAsync(CarRentDbContext context, decimal totalAmount = 200m)
    {
        var customerId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        await context.Users.AddAsync(new User { Id = customerId, FirstName = "Jane", LastName = "Doe", Email = $"{Guid.NewGuid():N}@example.com", RoleId = roleId });

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = $"REG-{Guid.NewGuid():N}",
            Vin = "VIN1",
            Year = 2024,
            FuelType = "Petrol",
            Transmission = "Automatic",
            SeatingCapacity = 5,
            DailyRate = 100,
            Status = "Available",
            BranchId = branchId
        };
        await context.Vehicles.AddAsync(vehicle);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicle.Id,
            BranchId = branchId,
            BookingType = "Online",
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow,
            SubtotalAmount = totalAmount,
            TotalAmount = totalAmount,
            Status = "Completed"
        };
        await context.Bookings.AddAsync(booking);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceNumber = "INV-TEST-0001",
            SubtotalAmount = totalAmount,
            TotalAmount = totalAmount,
            Status = "Unpaid"
        };
        await context.Invoices.AddAsync(invoice);

        await context.SaveChangesAsync();
        return (booking, invoice);
    }

    [Fact]
    public async Task RecordManualPaymentAsync_MarksInvoicePartiallyPaid_WhenAmountIsLessThanTotal()
    {
        await using var context = CreateContext();
        var (booking, invoice) = await SeedBookingWithInvoiceAsync(context, 200m);
        var service = new PaymentService(context, CreateGatewayService());

        await service.RecordManualPaymentAsync(new RecordManualPaymentRequest
        {
            BookingId = booking.Id,
            InvoiceId = invoice.Id,
            Amount = 100m,
            PaymentMethod = "Cash"
        }, Guid.NewGuid());

        var updated = await context.Invoices.AsNoTracking().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal("PartiallyPaid", updated.Status);
        Assert.Equal(100m, updated.AmountPaid);
    }

    [Fact]
    public async Task RecordManualPaymentAsync_MarksInvoicePaid_WhenCumulativePaymentsCoverTotal()
    {
        await using var context = CreateContext();
        var (booking, invoice) = await SeedBookingWithInvoiceAsync(context, 200m);
        var service = new PaymentService(context, CreateGatewayService());

        await service.RecordManualPaymentAsync(new RecordManualPaymentRequest { BookingId = booking.Id, InvoiceId = invoice.Id, Amount = 100m, PaymentMethod = "Cash" }, Guid.NewGuid());
        await service.RecordManualPaymentAsync(new RecordManualPaymentRequest { BookingId = booking.Id, InvoiceId = invoice.Id, Amount = 100m, PaymentMethod = "UPI" }, Guid.NewGuid());

        var updated = await context.Invoices.AsNoTracking().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal("Paid", updated.Status);
        Assert.Equal(200m, updated.AmountPaid);
    }

    [Fact]
    public async Task InitiateAsync_Throws_ForNonGatewayMethod()
    {
        await using var context = CreateContext();
        var (booking, _) = await SeedBookingWithInvoiceAsync(context);
        var service = new PaymentService(context, CreateGatewayService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InitiateAsync(new InitiatePaymentRequest { BookingId = booking.Id, Amount = 100, Gateway = "Cash" }));
    }

    [Fact]
    public async Task RecordManualPaymentAsync_Throws_ForGatewayMethod()
    {
        await using var context = CreateContext();
        var (booking, _) = await SeedBookingWithInvoiceAsync(context);
        var service = new PaymentService(context, CreateGatewayService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RecordManualPaymentAsync(new RecordManualPaymentRequest { BookingId = booking.Id, Amount = 100, PaymentMethod = "Razorpay" }, Guid.NewGuid()));
    }

    [Fact]
    public async Task InitiateThenConfirm_MarksPaymentVerified_AndRecalculatesInvoice()
    {
        await using var context = CreateContext();
        var (booking, invoice) = await SeedBookingWithInvoiceAsync(context, 150m);
        var service = new PaymentService(context, CreateGatewayService());

        var order = await service.InitiateAsync(new InitiatePaymentRequest { BookingId = booking.Id, InvoiceId = invoice.Id, Amount = 150m, Gateway = "Razorpay" });
        Assert.StartsWith("order_", order.GatewayOrderId);

        var confirmed = await service.ConfirmAsync(order.PaymentId, new ConfirmPaymentRequest { GatewayPaymentReference = "pay_test123" });
        Assert.Equal("Verified", confirmed.Status);

        var updatedInvoice = await context.Invoices.AsNoTracking().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal("Paid", updatedInvoice.Status);
    }

    [Theory]
    [InlineData("Karnataka", "Karnataka", 50, 50, 0)]
    [InlineData("Karnataka", "Maharashtra", 0, 0, 100)]
    [InlineData("Karnataka", null, 50, 50, 0)]
    public void GstSplitHelper_Split_ComputesCorrectBreakdown(string branchState, string? customerState, decimal expectedCgst, decimal expectedSgst, decimal expectedIgst)
    {
        var result = GstSplitHelper.Split(100m, branchState, customerState);

        Assert.Equal(expectedCgst, result.Cgst);
        Assert.Equal(expectedSgst, result.Sgst);
        Assert.Equal(expectedIgst, result.Igst);
    }

    [Fact]
    public async Task RefundApproval_ReducesInvoiceAmountPaid_AndRevertsStatus()
    {
        await using var context = CreateContext();
        var (booking, invoice) = await SeedBookingWithInvoiceAsync(context, 200m);
        var paymentService = new PaymentService(context, CreateGatewayService());

        var payment = await paymentService.RecordManualPaymentAsync(new RecordManualPaymentRequest { BookingId = booking.Id, InvoiceId = invoice.Id, Amount = 200m, PaymentMethod = "Cash" }, Guid.NewGuid());
        var paidInvoice = await context.Invoices.AsNoTracking().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal("Paid", paidInvoice.Status);

        var refundService = new RefundService(context, CreateGatewayService(), paymentService);
        var refund = await refundService.CreateAsync(new CreateRefundRequest { BookingId = booking.Id, PaymentId = payment.Id, Amount = 80m, Reason = "Customer complaint" }, Guid.NewGuid());
        await refundService.ApproveAsync(refund.Id, Guid.NewGuid());

        var updatedInvoice = await context.Invoices.AsNoTracking().FirstAsync(i => i.Id == invoice.Id);
        Assert.Equal(120m, updatedInvoice.AmountPaid);
        Assert.Equal("PartiallyPaid", updatedInvoice.Status);
    }

    [Fact]
    public async Task RefundService_CreateAsync_Throws_WhenPaymentNotVerified()
    {
        await using var context = CreateContext();
        var (booking, invoice) = await SeedBookingWithInvoiceAsync(context, 100m);
        var paymentService = new PaymentService(context, CreateGatewayService());

        var order = await paymentService.InitiateAsync(new InitiatePaymentRequest { BookingId = booking.Id, InvoiceId = invoice.Id, Amount = 100m, Gateway = "Stripe" });

        var refundService = new RefundService(context, CreateGatewayService(), paymentService);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            refundService.CreateAsync(new CreateRefundRequest { BookingId = booking.Id, PaymentId = order.PaymentId, Amount = 50m }, Guid.NewGuid()));
    }

    [Fact]
    public async Task RazorpayAndStripeProviders_ReturnWellFormedOrderIds()
    {
        var razorpay = new RazorpayGatewayProvider(NullLogger<RazorpayGatewayProvider>.Instance);
        var stripe = new StripeGatewayProvider(NullLogger<StripeGatewayProvider>.Instance);

        var razorpayOrder = await razorpay.CreateOrderAsync(100, "INR", "receipt-1");
        var stripeOrder = await stripe.CreateOrderAsync(100, "INR", "receipt-1");

        Assert.StartsWith("order_", razorpayOrder.OrderId);
        Assert.StartsWith("pi_", stripeOrder.OrderId);

        var razorpayVerify = await razorpay.VerifyPaymentAsync(razorpayOrder.OrderId, "pay_x", "sig_x");
        Assert.True(razorpayVerify.IsVerified);

        var razorpayRefund = await razorpay.RefundAsync("pay_x", 50);
        Assert.StartsWith("rfnd_", razorpayRefund.RefundReference);
    }
}
