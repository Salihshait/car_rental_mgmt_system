using CarRent.Application.Interfaces;
using CarRent.Application.Services;
using CarRent.Infrastructure.Persistence;
using CarRent.Infrastructure.Repositories;
using CarRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Linq;
using System.Net.Http;
using System.Text;

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
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInsuranceService, InsuranceService>();
builder.Services.AddScoped<DashboardService>();

var jwtKey = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "super-secret-key-please-change-me");
var supabaseAuthority = builder.Configuration["Supabase:Authority"]
    ?? Environment.GetEnvironmentVariable("SUPABASE_AUTHORITY")
    ?? "https://zjudixjgsyeglevlzrgp.supabase.co/auth/v1";
var supabaseAudience = builder.Configuration["Supabase:Audience"]
    ?? Environment.GetEnvironmentVariable("SUPABASE_AUDIENCE")
    ?? "authenticated";
var supabaseJwksUrl = builder.Configuration["Supabase:JwksUrl"]
    ?? Environment.GetEnvironmentVariable("SUPABASE_JWKS_URL")
    ?? "https://zjudixjgsyeglevlzrgp.supabase.co/auth/v1/.well-known/jwks.json";

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
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                using var httpClient = new HttpClient();
                var jwksJson = httpClient.GetStringAsync(supabaseJwksUrl).GetAwaiter().GetResult();
                var jwks = new JsonWebKeySet(jwksJson);
                return jwks.Keys.Where(key => string.Equals(key.Kid, kid, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        };
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
