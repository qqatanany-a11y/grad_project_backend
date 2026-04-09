using Event.Application.Services;
using Event.Application.Validators;
using Event.Infrastructure.Repos;
using events.domain.Entities;
using events.domain.Repos;
using events.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repos
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IRoleRepo, RoleRepo>();
builder.Services.AddScoped<IVenueRepo, VenueRepo>();
builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();  // ← موجود ✅

// Services
builder.Services.AddScoped<IAuthService, AuthService>();  // ← سيرفس وحدة بس
builder.Services.AddScoped<IVenueService, VenueService>();

// JWT
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

builder.Services.AddAuthorization();
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddValidatorsFromAssemblyContaining<RegisterVaildator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterOwnerVaildator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginVaildator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Users.Any())
    {
        var user = new User(
            "omar@gmail.com",
            BCrypt.Net.BCrypt.HashPassword("Omar1234"),
            "0782450024",
            "Omar",
            "Admin",
            "Naser",
            1 // RoleId for Admin

        );
      
        context.Users.Add(user);
        context.SaveChanges();
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();