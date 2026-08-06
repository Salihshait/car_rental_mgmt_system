using CarRent.Application.DTOs.Invoices;
using CarRent.Application.DTOs.Rentals;
using CarRent.Application.Interfaces;
using CarRent.Domain.Entities;
using CarRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Services;

public class RentalService : IRentalService
{
    private const string GraceMinutesSettingKey = "LateFeeGraceMinutes";
    private const string LateMultiplierSettingKey = "LateFeeDailyRateMultiplier";
    private const string DefaultDepositSettingKey = "DefaultSecurityDepositAmount";

    private const double DefaultGraceMinutes = 60;
    private const decimal DefaultLateMultiplier = 1.5m;
    private const decimal DefaultDepositAmount = 100m;

    private readonly CarRentDbContext _context;
    private readonly ISettingsService _settingsService;
    private readonly IInvoiceService _invoiceService;
    private readonly IRentalAgreementPdfService _pdfService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public RentalService(
        CarRentDbContext context,
        ISettingsService settingsService,
        IInvoiceService invoiceService,
        IRentalAgreementPdfService pdfService,
        INotificationService notificationService,
        IEmailService emailService,
        ISmsService smsService)
    {
        _context = context;
        _settingsService = settingsService;
        _invoiceService = invoiceService;
        _pdfService = pdfService;
        _notificationService = notificationService;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<IEnumerable<RentalSummaryDto>> GetAllAsync(RentalFilter filter, CancellationToken cancellationToken = default)
    {
        var query =
            from r in _context.Rentals.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
            join v in _context.Vehicles.AsNoTracking() on b.VehicleId equals v.Id
            select new { r, b, v };

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x => x.r.Status == filter.Status);
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(x => x.b.BranchId == filter.BranchId);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(x => x.r.PickupAt >= filter.DateFrom);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(x => x.r.PickupAt <= filter.DateTo);
        }

        return await query
            .OrderByDescending(x => x.r.PickupAt)
            .Select(x => new RentalSummaryDto
            {
                Id = x.r.Id,
                BookingId = x.r.BookingId,
                VehicleId = x.b.VehicleId,
                VehicleRegistrationNumber = x.v.RegistrationNumber,
                Status = x.r.Status,
                PickupAt = x.r.PickupAt,
                ReturnAt = x.r.ReturnAt,
                LateFeeAmount = x.r.LateFeeAmount,
                SecurityDepositAmount = x.r.SecurityDepositAmount,
                SecurityDepositStatus = x.r.SecurityDepositStatus
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        return rental is null ? null : await MapDetailAsync(rental, cancellationToken);
    }

    public async Task<RentalDto?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.BookingId == bookingId, cancellationToken);
        return rental is null ? null : await MapDetailAsync(rental, cancellationToken);
    }

    public async Task<RentalDto> PickupAsync(CreatePickupRequest request, Guid staffUserId, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status != "Confirmed")
        {
            throw new InvalidOperationException("Only confirmed bookings can be picked up.");
        }

        if (await _context.Rentals.AnyAsync(r => r.BookingId == request.BookingId, cancellationToken))
        {
            throw new InvalidOperationException("This booking already has a rental record.");
        }

        var depositAmount = request.SecurityDepositAmount ?? await GetDefaultDepositAsync(cancellationToken);

        var rental = new Rental
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Status = "Active",
            PickupAt = DateTime.UtcNow,
            PickupOdometerReading = request.OdometerReading,
            PickupFuelLevelPercent = request.FuelLevelPercent,
            PickupConditionNotes = request.ConditionNotes,
            PickupStaffUserId = staffUserId,
            SecurityDepositAmount = depositAmount,
            SecurityDepositStatus = depositAmount > 0 ? "Held" : "Refunded"
        };

        await _context.Rentals.AddAsync(rental, cancellationToken);

        if (depositAmount > 0)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = depositAmount,
                Purpose = "SecurityDeposit",
                PaymentMethod = "Cash",
                Gateway = "Manual",
                Status = "Held",
                PaidAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment, cancellationToken);
            rental.SecurityDepositPaymentId = payment.Id;
        }

        booking.Status = "Active";

        await WriteAuditAsync(booking.Id, "PickedUp", $"Vehicle picked up. Odometer {request.OdometerReading} km, fuel {request.FuelLevelPercent}%.", staffUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await NotifyAsync(booking.CustomerId, "Rental", "Your vehicle pickup is complete. Enjoy your rental!", "Vehicle Picked Up", cancellationToken);

        return await MapDetailAsync(rental, cancellationToken);
    }

    public async Task<byte[]> GenerateAgreementPdfAsync(Guid rentalId, byte[] signatureImageBytes, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == rental.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");
        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == booking.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Vehicle not found.");
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == booking.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException("Customer not found.");

        return _pdfService.Generate(rental, booking, vehicle, customer, signatureImageBytes);
    }

    public async Task<RentalDto> CompleteAgreementAsync(Guid rentalId, string signatureUrl, string pdfUrl, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");

        rental.AgreementSignedAt = DateTime.UtcNow;
        rental.AgreementSignatureUrl = signatureUrl;
        rental.AgreementPdfUrl = pdfUrl;

        await WriteAuditAsync(rental.BookingId, "AgreementSigned", "Rental agreement signed.", null, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(rental, cancellationToken);
    }

    public async Task<IEnumerable<RentalDamageDto>> GetDamagesAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return await _context.RentalDamages
            .AsNoTracking()
            .Where(d => d.RentalId == rentalId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new RentalDamageDto
            {
                Id = d.Id,
                RentalId = d.RentalId,
                Stage = d.Stage,
                Description = d.Description,
                Severity = d.Severity,
                EstimatedRepairCost = d.EstimatedRepairCost,
                ReportedBy = d.ReportedBy,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalDamageDto> AddDamageAsync(Guid rentalId, CreateRentalDamageRequest request, Guid reportedBy, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");

        var damage = new RentalDamage
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            Stage = request.Stage,
            Description = request.Description,
            Severity = request.Severity,
            EstimatedRepairCost = request.EstimatedRepairCost,
            ReportedBy = reportedBy
        };

        await _context.RentalDamages.AddAsync(damage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RentalDamageDto
        {
            Id = damage.Id,
            RentalId = damage.RentalId,
            Stage = damage.Stage,
            Description = damage.Description,
            Severity = damage.Severity,
            EstimatedRepairCost = damage.EstimatedRepairCost,
            ReportedBy = damage.ReportedBy,
            CreatedAt = damage.CreatedAt
        };
    }

    public async Task<IEnumerable<RentalPhotoDto>> GetPhotosAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return await _context.RentalPhotos
            .AsNoTracking()
            .Where(p => p.RentalId == rentalId)
            .OrderByDescending(p => p.UploadedAt)
            .Select(p => new RentalPhotoDto
            {
                Id = p.Id,
                RentalId = p.RentalId,
                Stage = p.Stage,
                Category = p.Category,
                StorageUrl = p.StorageUrl,
                RentalDamageId = p.RentalDamageId,
                UploadedBy = p.UploadedBy,
                UploadedAt = p.UploadedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalPhotoDto> AddPhotoAsync(Guid rentalId, string stage, string category, string storageUrl, Guid? rentalDamageId, Guid uploadedBy, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rentalDamageId.HasValue && !await _context.RentalDamages.AnyAsync(d => d.Id == rentalDamageId && d.RentalId == rental.Id, cancellationToken))
        {
            throw new InvalidOperationException("The referenced damage record does not belong to this rental.");
        }

        var photo = new RentalPhoto
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            Stage = stage,
            Category = category,
            StorageUrl = storageUrl,
            RentalDamageId = rentalDamageId,
            UploadedBy = uploadedBy
        };

        await _context.RentalPhotos.AddAsync(photo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RentalPhotoDto
        {
            Id = photo.Id,
            RentalId = photo.RentalId,
            Stage = photo.Stage,
            Category = photo.Category,
            StorageUrl = photo.StorageUrl,
            RentalDamageId = photo.RentalDamageId,
            UploadedBy = photo.UploadedBy,
            UploadedAt = photo.UploadedAt
        };
    }

    public async Task<IEnumerable<RentalChargeDto>> GetChargesAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        return await _context.RentalCharges
            .AsNoTracking()
            .Where(c => c.RentalId == rentalId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new RentalChargeDto
            {
                Id = c.Id,
                RentalId = c.RentalId,
                ChargeType = c.ChargeType,
                Description = c.Description,
                Amount = c.Amount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalChargeDto> AddChargeAsync(Guid rentalId, CreateRentalChargeRequest request, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");

        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Charge amount must be greater than zero.");
        }

        var charge = new RentalCharge
        {
            Id = Guid.NewGuid(),
            RentalId = rental.Id,
            ChargeType = request.ChargeType,
            Description = request.Description,
            Amount = request.Amount
        };

        await _context.RentalCharges.AddAsync(charge, cancellationToken);

        if (rental.Status == "Closed")
        {
            // The invoice was already generated at return; a manual charge added afterward (e.g. damage
            // discovered later) still needs to reach the customer's bill.
            var booking = await _context.Bookings.FirstAsync(b => b.Id == rental.BookingId, cancellationToken);
            booking.TotalAmount += request.Amount;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new RentalChargeDto
        {
            Id = charge.Id,
            RentalId = charge.RentalId,
            ChargeType = charge.ChargeType,
            Description = charge.Description,
            Amount = charge.Amount,
            CreatedAt = charge.CreatedAt
        };
    }

    public async Task<RentalDto> ReturnAsync(Guid rentalId, CreateReturnRequest request, Guid staffUserId, CancellationToken cancellationToken = default)
    {
        var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.Id == rentalId, cancellationToken)
            ?? throw new InvalidOperationException("Rental not found.");

        if (rental.Status != "Active")
        {
            throw new InvalidOperationException("This rental has already been closed.");
        }

        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == rental.BookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking not found.");

        var returnAt = request.ReturnAt ?? DateTime.UtcNow;

        rental.ReturnAt = returnAt;
        rental.ReturnOdometerReading = request.OdometerReading;
        rental.ReturnFuelLevelPercent = request.FuelLevelPercent;
        rental.ReturnConditionNotes = request.ConditionNotes;
        rental.ReturnStaffUserId = staffUserId;

        var (lateFee, lateHours) = await CalculateLateFeeAsync(booking, returnAt, cancellationToken);
        rental.LateFeeAmount = lateFee;
        rental.LateHours = lateHours;

        if (lateFee > 0)
        {
            await _context.RentalCharges.AddAsync(new RentalCharge
            {
                Id = Guid.NewGuid(),
                RentalId = rental.Id,
                ChargeType = "Late",
                Description = $"Late return by {lateHours} hour(s).",
                Amount = lateFee
            }, cancellationToken);
        }

        var damages = await _context.RentalDamages
            .Where(d => d.RentalId == rental.Id && d.EstimatedRepairCost > 0)
            .ToListAsync(cancellationToken);

        foreach (var damage in damages)
        {
            await _context.RentalCharges.AddAsync(new RentalCharge
            {
                Id = Guid.NewGuid(),
                RentalId = rental.Id,
                ChargeType = "Damage",
                Description = damage.Description,
                Amount = damage.EstimatedRepairCost
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var totalCharges = await _context.RentalCharges.Where(c => c.RentalId == rental.Id).SumAsync(c => c.Amount, cancellationToken);
        booking.TotalAmount += totalCharges;

        var depositAmount = rental.SecurityDepositAmount;
        var refundAmount = Math.Max(0, depositAmount - totalCharges);
        rental.SecurityDepositRefundAmount = refundAmount;
        rental.SecurityDepositStatus = depositAmount <= 0
            ? "Refunded"
            : refundAmount <= 0 ? "Forfeited"
            : refundAmount < depositAmount ? "PartiallyRefunded"
            : "Refunded";

        if (refundAmount > 0)
        {
            await _context.Refunds.AddAsync(new Refund
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = refundAmount,
                Status = "Processed",
                ProcessedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        rental.Status = "Closed";
        booking.Status = "Completed";

        await WriteAuditAsync(booking.Id, "Returned", $"Vehicle returned. Odometer {request.OdometerReading} km. Late fee {lateFee:C}. Deposit refund {refundAmount:C}.", staffUserId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var invoice = await _invoiceService.GenerateAsync(new CreateInvoiceRequest { BookingId = booking.Id }, cancellationToken);
            rental.FinalInvoiceId = invoice.Id;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // An invoice may already exist for this booking; return processing should still succeed.
        }

        await NotifyAsync(booking.CustomerId, "Rental", $"Your rental is complete. Total charges: {booking.TotalAmount:C}. Deposit refund: {refundAmount:C}.", "Rental Completed", cancellationToken);

        return await MapDetailAsync(rental, cancellationToken);
    }

    public async Task<RentalReportSummaryDto> GetReportSummaryAsync(DateTime? from, DateTime? to, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? DateTime.UtcNow.AddDays(-30);
        var effectiveTo = to ?? DateTime.UtcNow;

        var query =
            from r in _context.Rentals.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
            where r.PickupAt >= effectiveFrom && r.PickupAt <= effectiveTo
            select new { r, b };

        if (branchId.HasValue)
        {
            query = query.Where(x => x.b.BranchId == branchId);
        }

        var rentals = await query.Select(x => x.r).ToListAsync(cancellationToken);
        var rentalIds = rentals.Select(r => r.Id).ToList();

        var damageCharges = await _context.RentalCharges
            .AsNoTracking()
            .Where(c => rentalIds.Contains(c.RentalId) && c.ChargeType == "Damage")
            .SumAsync(c => (decimal?)c.Amount, cancellationToken) ?? 0;

        var closedRentals = rentals.Where(r => r.Status == "Closed" && r.ReturnAt.HasValue).ToList();

        return new RentalReportSummaryDto
        {
            TotalRentals = rentals.Count,
            ActiveRentals = rentals.Count(r => r.Status == "Active"),
            ClosedRentals = rentals.Count(r => r.Status == "Closed"),
            AverageTurnaroundHours = closedRentals.Count == 0
                ? 0
                : Math.Round(closedRentals.Average(r => (r.ReturnAt!.Value - r.PickupAt).TotalHours), 1),
            TotalLateFees = rentals.Sum(r => r.LateFeeAmount),
            TotalDamageCharges = damageCharges,
            TotalDepositsHeld = rentals.Where(r => r.SecurityDepositStatus == "Held").Sum(r => r.SecurityDepositAmount),
            TotalDepositsRefunded = rentals.Where(r => r.SecurityDepositStatus is "Refunded" or "PartiallyRefunded").Sum(r => r.SecurityDepositRefundAmount ?? 0),
            TotalDepositsForfeited = rentals.Where(r => r.SecurityDepositStatus == "Forfeited").Sum(r => r.SecurityDepositAmount)
        };
    }

    private async Task<(decimal LateFee, int LateHours)> CalculateLateFeeAsync(Booking booking, DateTime returnAt, CancellationToken cancellationToken)
    {
        if (returnAt <= booking.EndDate)
        {
            return (0, 0);
        }

        var graceRaw = await _settingsService.GetAsync(GraceMinutesSettingKey, cancellationToken);
        var graceMinutes = double.TryParse(graceRaw, out var parsedGrace) ? parsedGrace : DefaultGraceMinutes;

        var lateMinutes = (returnAt - booking.EndDate).TotalMinutes;
        if (lateMinutes <= graceMinutes)
        {
            return (0, 0);
        }

        var lateHours = (int)Math.Ceiling(lateMinutes / 60.0);
        var lateDays = Math.Max(1, (int)Math.Ceiling(lateHours / 24.0));

        var multiplierRaw = await _settingsService.GetAsync(LateMultiplierSettingKey, cancellationToken);
        var multiplier = decimal.TryParse(multiplierRaw, out var parsedMultiplier) ? parsedMultiplier : DefaultLateMultiplier;

        var vehicle = await _context.Vehicles.AsNoTracking().FirstAsync(v => v.Id == booking.VehicleId, cancellationToken);
        var lateFee = Math.Round(lateDays * vehicle.DailyRate * multiplier, 2);

        return (lateFee, lateHours);
    }

    private async Task<decimal> GetDefaultDepositAsync(CancellationToken cancellationToken)
    {
        var raw = await _settingsService.GetAsync(DefaultDepositSettingKey, cancellationToken);
        return decimal.TryParse(raw, out var value) ? value : DefaultDepositAmount;
    }

    private async Task<RentalDto> MapDetailAsync(Rental rental, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == rental.BookingId, cancellationToken);
        var vehicle = booking is null ? null : await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == booking.VehicleId, cancellationToken);
        var customer = booking is null ? null : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == booking.CustomerId, cancellationToken);

        return new RentalDto
        {
            Id = rental.Id,
            BookingId = rental.BookingId,
            CustomerId = booking?.CustomerId ?? Guid.Empty,
            CustomerName = customer is null ? null : $"{customer.FirstName} {customer.LastName}",
            VehicleId = booking?.VehicleId ?? Guid.Empty,
            VehicleRegistrationNumber = vehicle?.RegistrationNumber,
            Status = rental.Status,
            PickupAt = rental.PickupAt,
            PickupOdometerReading = rental.PickupOdometerReading,
            PickupFuelLevelPercent = rental.PickupFuelLevelPercent,
            PickupConditionNotes = rental.PickupConditionNotes,
            PickupStaffUserId = rental.PickupStaffUserId,
            AgreementSignedAt = rental.AgreementSignedAt,
            AgreementSignatureUrl = rental.AgreementSignatureUrl,
            AgreementPdfUrl = rental.AgreementPdfUrl,
            ReturnAt = rental.ReturnAt,
            ReturnOdometerReading = rental.ReturnOdometerReading,
            ReturnFuelLevelPercent = rental.ReturnFuelLevelPercent,
            ReturnConditionNotes = rental.ReturnConditionNotes,
            ReturnStaffUserId = rental.ReturnStaffUserId,
            SecurityDepositAmount = rental.SecurityDepositAmount,
            SecurityDepositStatus = rental.SecurityDepositStatus,
            SecurityDepositRefundAmount = rental.SecurityDepositRefundAmount,
            LateFeeAmount = rental.LateFeeAmount,
            LateHours = rental.LateHours,
            FinalInvoiceId = rental.FinalInvoiceId,
            CreatedAt = rental.CreatedAt
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

    private async Task NotifyAsync(Guid userId, string notificationType, string message, string emailSubject, CancellationToken cancellationToken)
    {
        await _notificationService.CreateAsync(userId, notificationType, message, cancellationToken);

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        await _emailService.SendAsync(user.Email, emailSubject, message, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            await _smsService.SendAsync(user.PhoneNumber, message, cancellationToken);
        }
    }
}
