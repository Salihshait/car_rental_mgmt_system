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

        base.OnModelCreating(modelBuilder);
    }
}
