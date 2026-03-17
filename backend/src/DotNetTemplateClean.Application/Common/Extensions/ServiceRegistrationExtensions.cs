using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application;

public static class ServiceRegistrationExtensions
{
       /// <summary>
    /// Registers all services automatically using reflection.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="assembly">The assembly to scan for repositories.</param>   
    public static IServiceCollection AddServices(
    this IServiceCollection services,
    Assembly assembly)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)

            // 🔹 On cible uniquement les services applicatifs
            .AddClasses(classes => classes
                .Where(type =>
                    type.Name.EndsWith( "Service", StringComparison.Ordinal) &&
                    type.IsClass &&
                    !type.IsAbstract))

            // 🔹 On map uniquement les interfaces métier
            .AsMatchingInterface() // IUserService ← UserService

            .WithScopedLifetime()
        );

        return services;
    }
}
