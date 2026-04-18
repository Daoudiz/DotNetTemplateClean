
using DotNetTemplateClean.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;


namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        // =========================================================================
        //                              JWT Authentication
        // =========================================================================
        // Cette ligne est magique : elle dit à .NET de garder les noms de claims originaux (ex: "role")
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"]; // Récupère la nouvelle ligne

        builder.Services
            .AddAuthentication(options =>
            {
                // On définit tout ici une seule fois
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Il sera 'false' en local (Dev) et 'true' sur le serveur (Prod)
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
                    ClockSkew = TimeSpan.Zero // pas de tolérance sur l'expiration
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Échec authentification : " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

        //Configure the ConnectionString and DbContext class
        var connectionString = builder.Configuration.GetConnectionString("DBConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
#if UseSqlServer
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,               // nombre max de tentatives
                    maxRetryDelay: TimeSpan.FromSeconds(10),  // délai max entre tentatives
                    errorNumbersToAdd: null          // codes SQL spécifiques à ajouter
                );
            });
#elif UsePostgreSQL
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
#else
            // Default to SQL Server if no symbol provided
            options.UseSqlServer(connectionString);
#endif
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        //Configure Identity
        builder.Services.AddIdentityCore<ApplicationUser>()
                       .AddRoles<IdentityRole>()
                       .AddSignInManager<SignInManager<ApplicationUser>>()
                       .AddEntityFrameworkStores<ApplicationDbContext>()
                       .AddDefaultTokenProviders()
                       .AddErrorDescriber<FrenchIdentityErrorDescriber>();

        builder.Services.ConfigueIdentity(builder.Configuration);

        //register JWT Token Service to generate tokens
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        //Register UserService for handling user-related operations (login, logout, profile)
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddScoped<IRoleService, RoleService>();
    }
}

