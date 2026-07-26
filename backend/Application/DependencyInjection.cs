using System;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MatdarSathi.API.Application.Common.Interfaces;

namespace MatdarSathi.API.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 1. Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // 2. Register native IRequestHandler<,> implementations automatically via assembly scanning
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Implementation = t, Interface = i })
            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        foreach (var h in handlerTypes)
        {
            services.AddScoped(h.Interface, h.Implementation);
        }

        return services;
    }
}
