using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MatdanSathi.API.Application.Common.Interfaces;
using MatdanSathi.API.Infrastructure.Persistence;
using MatdanSathi.API.Infrastructure.Security;

namespace MatdanSathi.API.Infrastructure;

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

        // 2. Register ApplicationDbContext referencing PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // 3. Bind the application db context interface to implementation
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
