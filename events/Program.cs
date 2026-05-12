using Event.Application.IServices;
using Event.Application.Services;
using Event.Application.Validators;
using Event.Infrastructure.Repos;
using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);
var connectionString = DatabaseConnectionStringResolver.Resolve(builder.Configuration);

// Render injects PORT at runtime, so bind Kestrel explicitly when it is present.
ConfigureRenderPort(builder);

// ================= DB =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ================= Repositories =================
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IRoleRepo, RoleRepo>();
builder.Services.AddScoped<IVenueRepo, VenueRepo>();
builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();
builder.Services.AddScoped<IOwnerRequestRepo, OwnerRequestRepo>();
builder.Services.AddScoped<IBookingRepo, BookingRepo>();
builder.Services.AddScoped<IEditRequestRepo, EditRequestRepo>();
builder.Services.AddScoped<IServiceRepo, ServiceRepo>();
builder.Services.AddScoped<IVenueServiceOptionRepo, VenueServiceOptionRepo>();
builder.Services.AddScoped<IBookingSelectedServiceRepo, BookingSelectedServiceRepo>();
builder.Services.AddScoped<IVenueAvailabilityRepo, VenueAvailabilityRepo>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IReviewRepo, ReviewRepo>();
// ================= Services =================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEditRequestService, EditRequestService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IVenueServiceOptionService, VenueServiceOptionService>();
builder.Services.AddScoped<IVenueAvailabilityService, VenueAvailabilityService>();
builder.Services.AddHostedService<BookingReminderBackgroundService>();
builder.Services.AddHostedService<BookingReminderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ReviewService>();
// ================= JWT =================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

/////////////////////////////// Rate Limiting for Login ///////////////////////////////
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many login attempts. Try again later.");
    };

    options.AddPolicy("Login", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(60)
            });
    });
});

// ================= Core =================
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// ================= CORS =================
var allowedCorsOrigins = BuildAllowedCorsOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClients", policy =>
    {
        // Keep local frontend origins and add the deployed frontend URL from configuration.
        policy.WithOrigins(allowedCorsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================= Validators =================
builder.Services.AddValidatorsFromAssemblyContaining<RegisterVaildator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterOwnerVaildator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginVaildator>();
builder.Services.AddScoped<IPasswordGenerator, PasswordGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVenueAvailabilityRepo, VenueAvailabilityRepo>();
var app = builder.Build();

// ================= Seed Admin =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!await context.Users.AnyAsync())
    {
        var user = new User(
            "admin@gmail.com",
            BCrypt.Net.BCrypt.HashPassword("Admin1234"),
            "0782450024",
            "Admin",
            "System",
            1
        );

        context.Users.Add(user);
        context.SaveChanges();
    }
}

// ================= Pipeline =================
var swaggerEnabledInProduction = builder.Configuration.GetValue<bool>("Swagger:EnableInProduction");
if (app.Environment.IsDevelopment() || swaggerEnabledInProduction)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendClients");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ConfigureRenderPort(WebApplicationBuilder builder)
{
    var portValue = builder.Configuration["PORT"];

    if (int.TryParse(portValue, out var port))
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }
}

static string[] BuildAllowedCorsOrigins(IConfiguration configuration)
{
    var origins = new List<string>
    {
        "http://localhost:5173",
        "http://localhost:3000"
    };

    var frontendUrl = configuration["FRONTEND_URL"];

    if (!string.IsNullOrWhiteSpace(frontendUrl))
    {
        origins.Add(frontendUrl.TrimEnd('/'));
    }

    return origins
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
