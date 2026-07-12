using System;
using System.Text;
using Asp.Versioning;
using Serilog;
using MatdanSathi.API.Application;
using MatdanSathi.API.Infrastructure;
using MatdanSathi.API.Infrastructure.Persistence;
using MatdanSathi.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using MatdanSathi.API.Application.Common.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Serilog Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

try
{
    Log.Information("Starting MatdanSathi backend API bootstrap...");

    // 2. Add Layer-Specific Services
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // 3. Add Controllers and Web API Services
    builder.Services.AddControllers();

    // 4. Configure JWT Authentication
    var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "super-secret-secure-key-for-matdansathi-jwt-validation-2026-auth";
    var key = Encoding.ASCII.GetBytes(jwtSecret);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "MatdanSathiAPI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "MatdanSathiClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    // 5. Configure API Versioning (strict /api/v1/)
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // 6. Configure RFC7807 Global Exception Handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // 7. Configure OpenAPI support
    builder.Services.AddOpenApi();

    // 8. Configure Anti-Scraping Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("strict-limit", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 30;
            opt.QueueLimit = 0;
        });
    });

    var app = builder.Build();

    // Database Migration and Seeding on Startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            var cryptoService = services.GetRequiredService<ICryptographyService>();
            DbInitializer.Initialize(dbContext, cryptoService);
            Log.Information("Database successfully migrated and seeded.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating or seeding the database.");
        }
    }

    // Configure HTTP Request Pipeline
    app.UseExceptionHandler(); // Invokes GlobalExceptionHandler

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MatdanSathi host terminated unexpectedly during bootstrap.");
}
finally
{
    Log.CloseAndFlush();
}
