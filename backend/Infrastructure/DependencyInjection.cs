using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MatdarSathi.API.Application.Common.Interfaces;
using MatdarSathi.API.Infrastructure.Persistence;
using MatdarSathi.API.Infrastructure.Security;
using MatdarSathi.API.Infrastructure.Common;

namespace MatdarSathi.API.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Configure Cryptography Option Binding and Register Cryptography Service
        services.Configure<CryptographySettings>(
            configuration.GetSection(CryptographySettings.SectionName));

        services.AddSingleton<ICryptographyService, CryptographyService>();

        // 2. Register ApplicationDbContext (Supports PostgreSQL with zero-config SQLite local fallback for developers)
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connStr = configuration.GetConnectionString("DefaultConnection") ?? "";
            var useSqlite = configuration.GetValue<bool>("UseSqlite") || string.IsNullOrWhiteSpace(connStr) || connStr.Contains("matdarsathi_dev.db");

            if (useSqlite || !connStr.Contains("Host="))
            {
                options.UseSqlite("Data Source=matdarsathi_dev.db",
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
            else
            {
                options.UseNpgsql(connStr,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            }
        });

        // 3. Bind the application db context interface to implementation
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // 4. Register Native MIT Mediator
        services.AddScoped<IMediator, NativeMediator>();

        // 5. Register Shared Watchdog Comparison Service and Background Roll Ingestion Worker
        services.AddScoped<IWatchdogComparisonService, MatdarSathi.API.Infrastructure.Services.WatchdogComparisonService>();
        services.AddHostedService<MatdarSathi.API.Infrastructure.Services.RollIngestionBackgroundService>();

        // 6. Register Messaging Channel Adapter Services (WhatsApp / SMS / Notification)
        services.Configure<MatdarSathi.API.Infrastructure.Messaging.MessagingSettings>(
            configuration.GetSection(MatdarSathi.API.Infrastructure.Messaging.MessagingSettings.SectionName));
        services.AddScoped<MatdarSathi.API.Infrastructure.Messaging.IMessagingChannel, MatdarSathi.API.Infrastructure.Messaging.TwilioMessagingChannel>();

        return services;
    }
}
