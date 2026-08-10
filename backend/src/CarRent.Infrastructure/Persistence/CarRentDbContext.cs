using CarRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRent.Infrastructure.Persistence;

public class CarRentDbContext : DbContext
{
    public CarRentDbContext(DbContextOptions<CarRentDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<VehicleCategory> VehicleCategories => Set<VehicleCategory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<FavoriteVehicle> FavoriteVehicles => Set<FavoriteVehicle>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<BookingExtension> BookingExtensions => Set<BookingExtension>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<RentalDamage> RentalDamages => Set<RentalDamage>();
    public DbSet<RentalPhoto> RentalPhotos => Set<RentalPhoto>();
    public DbSet<RentalCharge> RentalCharges => Set<RentalCharge>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<VehicleMaintenance> VehicleMaintenances => Set<VehicleMaintenance>();
    public DbSet<FuelLog> FuelLogs => Set<FuelLog>();
    public DbSet<VehicleLocation> VehicleLocations => Set<VehicleLocation>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<DriverAssignment> DriverAssignments => Set<DriverAssignment>();
    public DbSet<VehicleTransfer> VehicleTransfers => Set<VehicleTransfer>();
    public DbSet<DriverDocument> DriverDocuments => Set<DriverDocument>();
    public DbSet<DriverAttendance> DriverAttendances => Set<DriverAttendance>();
    public DbSet<DriverSalaryPayment> DriverSalaryPayments => Set<DriverSalaryPayment>();
    public DbSet<DriverRating> DriverRatings => Set<DriverRating>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<MaintenancePartUsage> MaintenancePartUsages => Set<MaintenancePartUsage>();
    public DbSet<VehicleWarranty> VehicleWarranties => Set<VehicleWarranty>();
    public DbSet<AmcContract> AmcContracts => Set<AmcContract>();
    public DbSet<VehicleInspection> VehicleInspections => Set<VehicleInspection>();
    public DbSet<MaintenanceExpense> MaintenanceExpenses => Set<MaintenanceExpense>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportTicketMessage> SupportTicketMessages => Set<SupportTicketMessage>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<Feedback> FeedbackEntries => Set<Feedback>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PlanLimit> PlanLimits => Set<PlanLimit>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<TenantFeatureOverride> TenantFeatureOverrides => Set<TenantFeatureOverride>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<TenantUsageMetric> TenantUsageMetrics => Set<TenantUsageMetric>();
    public DbSet<TenantBranding> TenantBrandings => Set<TenantBranding>();
    public DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<MaintenancePrediction> MaintenancePredictions => Set<MaintenancePrediction>();
    public DbSet<DamageDetectionResult> DamageDetectionResults => Set<DamageDetectionResult>();
    public DbSet<DocumentOcrResult> DocumentOcrResults => Set<DocumentOcrResult>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<VoiceBookingRequest> VoiceBookingRequests => Set<VoiceBookingRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Branch>()
            .HasIndex(b => b.Name)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<VehicleCategory>()
            .HasIndex(c => c.Name)
            .IsUnique();

        modelBuilder.Entity<Brand>()
            .HasIndex(b => b.Name)
            .IsUnique();

        modelBuilder.Entity<Model>()
            .HasOne(m => m.Brand)
            .WithMany()
            .HasForeignKey(m => m.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Model>()
            .HasOne(m => m.Category)
            .WithMany()
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Brand)
            .WithMany()
            .HasForeignKey(v => v.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.VehicleModel)
            .WithMany()
            .HasForeignKey(v => v.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Payload)
            .HasColumnType("jsonb");

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.UserId)
            .IsUnique();

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FavoriteVehicle>()
            .HasKey(f => new { f.CustomerId, f.VehicleId });

        modelBuilder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<Setting>()
            .HasIndex(s => s.KeyName)
            .IsUnique();

        modelBuilder.Entity<Booking>()
            .HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(b => b.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CouponRedemption>()
            .HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(r => r.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CouponRedemption>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BookingExtension>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rental>()
            .HasIndex(r => r.BookingId)
            .IsUnique();

        modelBuilder.Entity<Rental>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RentalDamage>()
            .HasOne<Rental>()
            .WithMany()
            .HasForeignKey(d => d.RentalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RentalPhoto>()
            .HasOne<Rental>()
            .WithMany()
            .HasForeignKey(p => p.RentalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RentalCharge>()
            .HasOne<Rental>()
            .WithMany()
            .HasForeignKey(c => c.RentalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Refund>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Refund>()
            .HasOne<Payment>()
            .WithMany()
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Payment>()
            .HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Driver>()
            .HasIndex(d => d.UserId)
            .IsUnique();

        modelBuilder.Entity<Driver>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleMaintenance>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FuelLog>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleLocation>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleLocation>()
            .HasOne<Trip>()
            .WithMany()
            .HasForeignKey(l => l.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Trip>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Trip>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DriverAssignment>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverAssignment>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(a => a.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VehicleTransfer>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverDocument>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverAttendance>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(a => a.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverAttendance>()
            .HasIndex(a => new { a.DriverId, a.AttendanceDate })
            .IsUnique();

        modelBuilder.Entity<DriverSalaryPayment>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(s => s.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverRating>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SparePart>()
            .HasIndex(p => p.PartNumber)
            .IsUnique();

        modelBuilder.Entity<Workshop>()
            .HasOne<Vendor>()
            .WithMany()
            .HasForeignKey(w => w.VendorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<VehicleMaintenance>()
            .HasOne<Workshop>()
            .WithMany()
            .HasForeignKey(m => m.WorkshopId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MaintenancePartUsage>()
            .HasOne<VehicleMaintenance>()
            .WithMany()
            .HasForeignKey(u => u.MaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaintenancePartUsage>()
            .HasOne<SparePart>()
            .WithMany()
            .HasForeignKey(u => u.SparePartId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleWarranty>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(w => w.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AmcContract>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(c => c.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AmcContract>()
            .HasOne<Vendor>()
            .WithMany()
            .HasForeignKey(c => c.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleInspection>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaintenanceExpense>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(e => e.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SupportTicket>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SupportTicketMessage>()
            .HasOne<SupportTicket>()
            .WithMany()
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Complaint>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Complaint>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(c => c.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Complaint>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(c => c.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Feedback>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Feedback>()
            .HasOne<Booking>()
            .WithMany()
            .HasForeignKey(f => f.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Campaign>()
            .HasOne<MessageTemplate>()
            .WithMany()
            .HasForeignKey(c => c.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MessageLog>()
            .HasOne<MessageTemplate>()
            .WithMany()
            .HasForeignKey(l => l.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MessageLog>()
            .HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(l => l.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BankAccount>()
            .HasOne<Branch>()
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BankTransaction>()
            .HasOne<BankAccount>()
            .WithMany()
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JournalEntry>()
            .HasOne<BankAccount>()
            .WithMany()
            .HasForeignKey(j => j.BankAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        modelBuilder.Entity<PlanLimit>()
            .HasOne<SubscriptionPlan>()
            .WithMany()
            .HasForeignKey(l => l.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlanFeature>()
            .HasOne<SubscriptionPlan>()
            .WithMany()
            .HasForeignKey(f => f.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantFeatureOverride>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(o => o.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne<SubscriptionPlan>()
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubscriptionInvoice>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(i => i.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SubscriptionInvoice>()
            .HasOne<Subscription>()
            .WithMany()
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TenantUsageMetric>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantBranding>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TenantBranding>()
            .HasIndex(b => b.TenantId)
            .IsUnique();

        modelBuilder.Entity<TenantDomain>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaintenancePrediction>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DamageDetectionResult>()
            .HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ChatMessage>()
            .HasOne<ChatSession>()
            .WithMany()
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}
