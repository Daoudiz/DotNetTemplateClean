
using DotNetTemplateClean.Application;
using DotNetTemplateClean.WebAPI;

using Microsoft.OpenApi.Models;


namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // =========================================================================
        //  configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        // =========================================================================

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            // Déclaration du schéma JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Entrez votre token JWT sous la forme : Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new  OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

       

       


        //Configuration du nombre de résultats à partir duquel on bascule en mode pagination
        builder.Services.AddOptions<SearchSettings>()
            .Bind(builder.Configuration.GetSection("SearchSettings"))
            .Validate(config => config.ThresholdForFullLoad > 0, "Le seuil de recherche doit être supérieur à 0");

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        builder.Services.AddEndpointsApiExplorer();
    }
}
