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
