
using System.Reflection;
using DotNetTemplateClean.Application;
using Microsoft.Extensions.Hosting;
using FluentValidation;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        //register services Default: Scoped lifetime
        var serviceAssembly = typeof(IEntiteService).Assembly;
        builder.Services.AddServices(serviceAssembly);

        builder.Services.AddAutoMapper(cfg =>
           cfg.AddMaps(Assembly.GetExecutingAssembly()));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }

}
