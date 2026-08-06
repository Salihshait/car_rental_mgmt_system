using CarRent.Application.DTOs.Bookings;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class BookingService : IBookingService
{
    private const string RequireApprovalSettingKey = "RequireBookingApproval";

    public static readonly string[] ActiveStatuses = { "PendingApproval", "Confirmed" };

    private readonly CarRentDbContext _context;
    private readonly IAvailabilityService _availabilityService;
    private readonly IPricingService _pricingService;
    private readonly ICouponService _couponService;
    private readonly IWaitlistService _waitlistService;
    private readonly IBookingNotificationService _bookingNotificationService;
    private readonly ISettingsService _settingsService;

    public BookingService(
        CarRentDbContext context,
        IAvailabilityService availabilityService,
        IPricingService pricingService,
        ICouponService couponService,
        IWaitlistService waitlistService,
        IBookingNotificationService bookingNotificationService,
        ISettingsService settingsService)
    {
        _context = context;
        _availabilityService = availabilityService;
        _pricingService = pricingService;
        _couponService = couponService;
        _waitlistService = waitlistService;
        _bookingNotificationService = bookingNotificationService;
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<BookingSummaryDto>> GetAllAsync(BookingFilter filter, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var effectiveCustomerId = isAdmin ? filter.CustomerId : callerUserId;

        var query = _context.Bookings.AsNoTracking().AsQueryable();

        if (effectiveCustomerId.HasValue)
        {
            query = query.Where(b => b.CustomerId == effectiveCustomerId);
        }

        if (filter.VehicleId.HasValue)
        {
            query = query.Where(b => b.VehicleId == filter.VehicleId);
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(b => b.BranchId == filter.BranchId);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(b => b.Status == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.BookingType))
        {
            query = query.Where(b => b.BookingType == filter.BookingType);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(b => b.EndDate >= filter.DateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(b => b.StartDate <= filter.DateTo);
        }

        return await query
            .Join(_context.Vehicles, b => b.VehicleId, v => v.Id, (b, v) => new BookingSummaryDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                VehicleId = b.VehicleId,
                VehicleRegistrationNumber = v.RegistrationNumber,
                BranchId = b.BranchId,
                BookingType = b.BookingType,
                BookingDate = b.BookingDate,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                SubtotalAmount = b.SubtotalAmount,
                DiscountAmount = b.DiscountAmount,
                TaxAmount = b.TaxAmount,
                TotalAmount = b.TotalAmount,
                Status = b.Status
            })
            .OrderByDescending(dto => dto.BookingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<BookingDto?> GetByIdAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (booking is null || (!isAdmin && booking.CustomerId != callerUserId))
        {
            return null;
        }

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<IEnumerable<BookingTimelineEntryDto>> GetTimelineAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        await GetOwnedBookingAsync(id, callerUserId, isAdmin, cancellationToken);

        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Booking" && a.EntityId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new BookingTimelineEntryDto { Id = a.Id, Action = a.Action, Payload = a.Payload, CreatedAt = a.CreatedAt })
            .ToListAsync(cancellationToken);
    }

    public async Task<PricingBreakdownDto> QuoteAsync(BookingQuoteRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("The selected vehicle does not exist.");

        return await _pricingService.CalculateAsync(vehicle.DailyRate, request.StartDate, request.EndDate, request.CouponCode, cancellationToken);
    }

    public Task<BookingDto> CreateOnlineAsync(Guid customerId, CreateOnlineBookingRequest request, CancellationToken cancellationToken = default) =>
        CreateInternalAsync(customerId, customerId, "Online", request.VehicleId, request.BranchId, request.ReturnBranchId, request.StartDate, request.EndDate, request.CouponCode, request.Notes, cancellationToken);

    public async Task<BookingDto> CreateWalkInAsync(Guid staffUserId, CreateWalkInBookingRequest request, CancellationToken cancellationToken = default)
    {
        var customerExists = await _context.Users.AnyAsync(u => u.Id == request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            throw new InvalidOperationException("Customer not found. Create the customer record first.");
        }

        return await CreateInternalAsync(request.CustomerId, staffUserId, "WalkIn", request.VehicleId, request.BranchId, request.ReturnBranchId, request.StartDate, request.EndDate, request.CouponCode, request.Notes, cancellationToken);
    }

    public async Task<BookingDto> UpdateAsync(Guid id, UpdateBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var booking = await GetOwnedBookingAsync(id, callerUserId, isAdmin, cancellationToken);

        if (!ActiveStatuses.Contains(booking.Status))
        {
            throw new InvalidOperationException("Only pending or confirmed bookings can be modified.");
        }

        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        var available = await _availabilityService.IsVehicleAvailableAsync(booking.VehicleId, request.StartDate, request.EndDate, booking.Id, cancellationToken);
        if (!available)
        {
            throw new InvalidOperationException("The vehicle is not available for the requested dates.");
        }

        var vehicle = await _context.Vehicles.FirstAsync(v => v.Id == booking.VehicleId, cancellationToken);
        var existingCouponCode = await GetCouponCodeAsync(booking.CouponId, cancellationToken);
        var pricing = await _pricingService.CalculateAsync(vehicle.DailyRate, request.StartDate, request.EndDate, existingCouponCode, cancellationToken);

        var previousStart = booking.StartDate;
        var previousEnd = booking.EndDate;

        booking.StartDate = request.StartDate;
        booking.EndDate = request.EndDate;
        booking.Notes = request.Notes;
        booking.SubtotalAmount = pricing.SubtotalAmount;
        booking.DiscountAmount = pricing.DiscountAmount;
        booking.TaxAmount = pricing.TaxAmount;
        booking.TotalAmount = pricing.TotalAmount;

        await WriteAuditAsync(booking.Id, "Modified", $"Dates changed from {previousStart:d}-{previousEnd:d} to {request.StartDate:d}-{request.EndDate:d}.", callerUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<BookingDto> CancelAsync(Guid id, CancelBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var booking = await GetOwnedBookingAsync(id, callerUserId, isAdmin, cancellationToken);

        if (booking.Status is "Cancelled" or "Completed" or "Rejected")
        {
            throw new InvalidOperationException("This booking cannot be cancelled.");
        }

        booking.Status = "Cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = request.Reason;

        await WriteAuditAsync(booking.Id, "Cancelled", request.Reason ?? "Booking cancelled.", callerUserId, cancellationToken);

        var verifiedPaymentsTotal = await _context.Payments
            .Where(p => p.BookingId == booking.Id && p.Status == "Verified")
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        if (verifiedPaymentsTotal > 0)
        {
            await _context.Refunds.AddAsync(new Refund
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = verifiedPaymentsTotal,
                Reason = "Booking cancelled.",
                Status = "Requested",
                RequestedBy = callerUserId
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _bookingNotificationService.NotifyBookingCancelledAsync(booking, cancellationToken);
        await _waitlistService.NotifyMatchingEntriesAsync(booking.VehicleId, booking.StartDate, booking.EndDate, cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<BookingDto> ApproveAsync(Guid id, Guid approverUserId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status != "PendingApproval")
        {
            throw new InvalidOperationException("Only bookings pending approval can be approved.");
        }

        booking.Status = "Confirmed";
        booking.ApprovedBy = approverUserId;
        booking.ApprovedAt = DateTime.UtcNow;

        await WriteAuditAsync(booking.Id, "Approved", "Booking approved.", approverUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _bookingNotificationService.NotifyBookingApprovedAsync(booking, cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<BookingDto> RejectAsync(Guid id, RejectBookingRequest request, Guid approverUserId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status != "PendingApproval")
        {
            throw new InvalidOperationException("Only bookings pending approval can be rejected.");
        }

        booking.Status = "Rejected";
        booking.CancellationReason = request.Reason;
        booking.ApprovedBy = approverUserId;
        booking.ApprovedAt = DateTime.UtcNow;

        await WriteAuditAsync(booking.Id, "Rejected", request.Reason ?? "Booking rejected.", approverUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _bookingNotificationService.NotifyBookingRejectedAsync(booking, cancellationToken);
        await _waitlistService.NotifyMatchingEntriesAsync(booking.VehicleId, booking.StartDate, booking.EndDate, cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<BookingDto> ExtendAsync(Guid id, ExtendBookingRequest request, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var booking = await GetOwnedBookingAsync(id, callerUserId, isAdmin, cancellationToken);

        if (booking.Status != "Confirmed")
        {
            throw new InvalidOperationException("Only confirmed bookings can be extended.");
        }

        if (request.NewEndDate <= booking.EndDate)
        {
            throw new InvalidOperationException("The new end date must be after the current end date.");
        }

        var available = await _availabilityService.IsVehicleAvailableAsync(booking.VehicleId, booking.EndDate, request.NewEndDate, booking.Id, cancellationToken);
        if (!available)
        {
            throw new InvalidOperationException("The vehicle is booked by another reservation during the requested extension period.");
        }

        var vehicle = await _context.Vehicles.FirstAsync(v => v.Id == booking.VehicleId, cancellationToken);
        var existingCouponCode = await GetCouponCodeAsync(booking.CouponId, cancellationToken);
        var newPricing = await _pricingService.CalculateAsync(vehicle.DailyRate, booking.StartDate, request.NewEndDate, existingCouponCode, cancellationToken);

        var previousEnd = booking.EndDate;
        var previousTotal = booking.TotalAmount;

        booking.EndDate = request.NewEndDate;
        booking.SubtotalAmount = newPricing.SubtotalAmount;
        booking.DiscountAmount = newPricing.DiscountAmount;
        booking.TaxAmount = newPricing.TaxAmount;
        booking.TotalAmount = newPricing.TotalAmount;

        await _context.BookingExtensions.AddAsync(new BookingExtension
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            PreviousEndDate = previousEnd,
            NewEndDate = request.NewEndDate,
            AdditionalAmount = newPricing.TotalAmount - previousTotal,
            Status = "Approved",
            Reason = request.Reason,
            RequestedBy = callerUserId,
            DecidedAt = DateTime.UtcNow
        }, cancellationToken);

        await WriteAuditAsync(booking.Id, "Extended", $"End date extended from {previousEnd:d} to {request.NewEndDate:d}.", callerUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _bookingNotificationService.NotifyBookingExtendedAsync(booking, cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    public async Task<BookingReportSummaryDto> GetReportSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddDays(-30);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.BookingDate >= effectiveFrom && b.BookingDate <= effectiveTo);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId);
        }

        var bookings = await query.ToListAsync(cancellationToken);
        var revenue = bookings.Where(b => b.Status != "Cancelled" && b.Status != "Rejected").Sum(b => b.TotalAmount);
        var revenueCount = bookings.Count(b => b.Status != "Cancelled" && b.Status != "Rejected");

        var vehicleIds = bookings.Select(b => b.VehicleId).Distinct().ToList();
        var vehicles = await _context.Vehicles.AsNoTracking().Where(v => vehicleIds.Contains(v.Id)).ToListAsync(cancellationToken);

        var topVehicles = bookings
            .GroupBy(b => b.VehicleId)
            .Select(g => new TopVehicleDto
            {
                VehicleId = g.Key,
                RegistrationNumber = vehicles.FirstOrDefault(v => v.Id == g.Key)?.RegistrationNumber,
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status != "Cancelled" && b.Status != "Rejected").Sum(b => b.TotalAmount)
            })
            .OrderByDescending(v => v.BookingCount)
            .Take(5)
            .ToList();

        return new BookingReportSummaryDto
        {
            TotalBookings = bookings.Count,
            TotalRevenue = revenue,
            AverageBookingValue = revenueCount == 0 ? 0 : Math.Round(revenue / revenueCount, 2),
            TotalDiscountGiven = bookings.Sum(b => b.DiscountAmount),
            StatusCounts = bookings.GroupBy(b => b.Status).ToDictionary(g => g.Key, g => g.Count()),
            BookingTypeCounts = bookings.GroupBy(b => b.BookingType).ToDictionary(g => g.Key, g => g.Count()),
            TopVehicles = topVehicles
        };
    }

    private async Task<BookingDto> CreateInternalAsync(
        Guid customerId,
        Guid createdByUserId,
        string bookingType,
        Guid vehicleId,
        Guid? branchId,
        Guid? returnBranchId,
        DateTime startDate,
        DateTime endDate,
        string? couponCode,
        string? notes,
        CancellationToken cancellationToken)
    {
        if (endDate <= startDate)
        {
            throw new InvalidOperationException("End date must be after start date.");
        }

        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken)
            ?? throw new InvalidOperationException("The selected vehicle does not exist.");

        if (vehicle.Status is "Maintenance" or "Accident")
        {
            throw new InvalidOperationException("This vehicle is currently out of service.");
        }

        var available = await _availabilityService.IsVehicleAvailableAsync(vehicleId, startDate, endDate, null, cancellationToken);
        if (!available)
        {
            throw new InvalidOperationException("The selected vehicle is not available for the requested dates.");
        }

        var pricing = await _pricingService.CalculateAsync(vehicle.DailyRate, startDate, endDate, couponCode, cancellationToken);

        var requiresApproval = bookingType == "Online" && await IsApprovalRequiredAsync(cancellationToken);
        var status = requiresApproval ? "PendingApproval" : "Confirmed";

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            BranchId = branchId ?? vehicle.BranchId,
            ReturnBranchId = returnBranchId,
            BookingDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            BookingType = bookingType,
            SubtotalAmount = pricing.SubtotalAmount,
            DiscountAmount = pricing.DiscountAmount,
            TaxAmount = pricing.TaxAmount,
            TotalAmount = pricing.TotalAmount,
            Status = status,
            Notes = notes,
            CreatedBy = createdByUserId
        };

        Coupon? coupon = null;
        if (pricing.CouponCode is not null)
        {
            coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == pricing.CouponCode, cancellationToken);
            booking.CouponId = coupon?.Id;
        }

        await _context.Bookings.AddAsync(booking, cancellationToken);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("The selected vehicle was just booked for an overlapping period. Please choose different dates.");
        }

        if (coupon is not null)
        {
            await _couponService.RedeemAsync(coupon.Id, booking.Id, customerId, pricing.DiscountAmount, cancellationToken);
        }

        await WriteAuditAsync(booking.Id, "Created", $"{bookingType} booking created with status {status}.", createdByUserId, cancellationToken);

        await _bookingNotificationService.NotifyBookingCreatedAsync(booking, cancellationToken);

        return await MapDetailAsync(booking, cancellationToken);
    }

    private async Task<Booking> GetOwnedBookingAsync(Guid id, Guid callerUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (booking is null || (!isAdmin && booking.CustomerId != callerUserId))
        {
            throw new InvalidOperationException("Booking not found.");
        }

        return booking;
    }

    private async Task<bool> IsApprovalRequiredAsync(CancellationToken cancellationToken)
    {
        var raw = await _settingsService.GetAsync(RequireApprovalSettingKey, cancellationToken);
        return !bool.TryParse(raw, out var value) || value;
    }

    private async Task<string?> GetCouponCodeAsync(Guid? couponId, CancellationToken cancellationToken)
    {
        if (!couponId.HasValue)
        {
            return null;
        }

        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);
        return coupon?.Code;
    }

    private async Task<BookingDto> MapDetailAsync(Booking booking, CancellationToken cancellationToken)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Brand)
            .Include(v => v.VehicleModel)
            .Include(v => v.Branch)
            .FirstOrDefaultAsync(v => v.Id == booking.VehicleId, cancellationToken);

        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == booking.CustomerId, cancellationToken);

        var couponCode = await GetCouponCodeAsync(booking.CouponId, cancellationToken);

        var taxRate = booking.SubtotalAmount - booking.DiscountAmount == 0
            ? 0
            : Math.Round(booking.TaxAmount / (booking.SubtotalAmount - booking.DiscountAmount) * 100m, 2);

        return new BookingDto
        {
            Id = booking.Id,
            CustomerId = booking.CustomerId,
            CustomerName = customer is null ? null : $"{customer.FirstName} {customer.LastName}",
            CustomerEmail = customer?.Email,
            VehicleId = booking.VehicleId,
            VehicleRegistrationNumber = vehicle?.RegistrationNumber,
            BrandName = vehicle?.Brand?.Name,
            ModelName = vehicle?.VehicleModel?.Name,
            BranchId = booking.BranchId,
            BranchName = vehicle?.Branch?.Name,
            ReturnBranchId = booking.ReturnBranchId,
            BookingType = booking.BookingType,
            BookingDate = booking.BookingDate,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            Status = booking.Status,
            Notes = booking.Notes,
            Pricing = new PricingBreakdownDto
            {
                Days = Math.Max(1, (int)Math.Ceiling((booking.EndDate - booking.StartDate).TotalDays)),
                DailyRate = vehicle?.DailyRate ?? 0,
                SubtotalAmount = booking.SubtotalAmount,
                CouponCode = couponCode,
                DiscountAmount = booking.DiscountAmount,
                TaxRatePercent = taxRate,
                TaxAmount = booking.TaxAmount,
                TotalAmount = booking.TotalAmount
            },
            CancelledAt = booking.CancelledAt,
            CancellationReason = booking.CancellationReason,
            ApprovedBy = booking.ApprovedBy,
            ApprovedAt = booking.ApprovedAt,
            CreatedBy = booking.CreatedBy
        };
    }

    private async Task WriteAuditAsync(Guid bookingId, string action, string message, Guid? actingUserId, CancellationToken cancellationToken)
    {
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = actingUserId,
            Action = action,
            EntityType = "Booking",
            EntityId = bookingId,
            Payload = $"{{\"message\":\"{message.Replace("\"", "'")}\"}}"
        }, cancellationToken);
    }
}
