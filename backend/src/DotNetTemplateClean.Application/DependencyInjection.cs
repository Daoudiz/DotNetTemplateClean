
using System.Reflection;

using DotNetTemplateClean.Application;

using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        //register services Default: Scoped lifetime
        var serviceAssembly = Assembly.GetExecutingAssembly();
        builder.Services.AddServices(serviceAssembly);

        builder.Services.AddAutoMapper(cfg =>
           cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Services.AddScoped<ITemporaryPasswordGenerator, TemporaryPasswordGenerator>();

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Pipeline order is intentional:
            // UnhandledException wraps the chain, then Authorization, Validation, and Performance.
            // Keep this order stable to preserve fail-fast authorization/validation and consistent timing/exception behavior.
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }

}
