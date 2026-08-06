using CarRent.Application.Interfaces;
using CarRent.Application.Services;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Repositories;
using CarRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Linq;
using System.Net.Http;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/car-rent-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SUPABASE_DB_CONNECTION");

builder.Services.AddDbContext<CarRentDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IVehicleCatalogService, VehicleCatalogService>();
builder.Services.AddScoped<IVehicleDocumentService, VehicleDocumentService>();
builder.Services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IWaitlistService, WaitlistService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailService, LoggingEmailService>();
builder.Services.AddScoped<ISmsService, LoggingSmsService>();
builder.Services.AddScoped<IBookingNotificationService, BookingNotificationService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IRentalAgreementPdfService, RentalAgreementPdfService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleMaintenanceService, VehicleMaintenanceService>();
builder.Services.AddScoped<IFuelLogService, FuelLogService>();
builder.Services.AddScoped<IFleetTrackingService, FleetTrackingService>();
builder.Services.AddScoped<IDriverAssignmentService, DriverAssignmentService>();
builder.Services.AddScoped<IVehicleTransferService, VehicleTransferService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<IDriverDocumentService, DriverDocumentService>();
builder.Services.AddScoped<IDriverAttendanceService, DriverAttendanceService>();
builder.Services.AddScoped<IDriverSalaryService, DriverSalaryService>();
builder.Services.AddScoped<IDriverRatingService, DriverRatingService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IWorkshopService, WorkshopService>();
builder.Services.AddScoped<ISparePartService, SparePartService>();
builder.Services.AddScoped<IVehicleWarrantyService, VehicleWarrantyService>();
builder.Services.AddScoped<IAmcContractService, AmcContractService>();
builder.Services.AddScoped<IVehicleInspectionService, VehicleInspectionService>();
builder.Services.AddScoped<IMaintenanceExpenseService, MaintenanceExpenseService>();
builder.Services.AddScoped<IMaintenanceReportService, MaintenanceReportService>();
builder.Services.AddScoped<IPaymentGatewayProvider, RazorpayGatewayProvider>();
builder.Services.AddScoped<IPaymentGatewayProvider, StripeGatewayProvider>();
builder.Services.AddScoped<IPaymentGatewayProvider, CashGatewayProvider>();
builder.Services.AddScoped<IPaymentGatewayProvider, UpiGatewayProvider>();
builder.Services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<IInvoicePdfService, InvoicePdfService>();
builder.Services.AddScoped<IBillingReportService, BillingReportService>();
builder.Services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();
builder.Services.AddHttpClient<ISupabaseAdminClient, SupabaseAdminClient>();
builder.Services.AddScoped<DashboardService>();

var supabaseUrl = (builder.Configuration["Supabase:Url"]
    ?? Environment.GetEnvironmentVariable("SUPABASE_URL")
    ?? "https://zjudixjgsyeglevlzrgp.supabase.co").TrimEnd('/');
var supabaseAuthority = $"{supabaseUrl}/auth/v1";
var supabaseAudience = "authenticated";
var supabaseJwksUrl = $"{supabaseAuthority}/.well-known/jwks.json";

var jwksProvider = new SupabaseJwksProvider(new MemoryCache(new MemoryCacheOptions()), new HttpClient(), supabaseJwksUrl);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = supabaseAuthority;
        options.Audience = supabaseAudience;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = supabaseAuthority,
            ValidAudience = supabaseAudience,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => jwksProvider.GetSigningKeys(kid)
        };
    });

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
        ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.SetIsOriginAllowed(origin =>
                  allowedOrigins.Contains(origin) ||
                  (Uri.TryCreate(origin, UriKind.Absolute, out var originUri) &&
                   originUri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase) &&
                   originUri.Host.StartsWith("car-rental-mgmt-system", StringComparison.OrdinalIgnoreCase)))
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Car Rental Management System API",
        Version = "v1",
        Description = "Production-ready API surface for a commercial car rental management platform."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
