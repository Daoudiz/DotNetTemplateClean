using DotNetTemplateClean.Infrastructure;
using DotNetTemplateClean.WebAPI;

using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

// =========================================================================
//                              CORS
// =========================================================================
const string AllowSpecificOriginsName = "AllowAngularDev";
builder.Services.AddCors(options =>
{

    options.AddPolicy(name: AllowSpecificOriginsName, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins")
                                  .Get<string[]>() ?? [];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();

        if (builder.Environment.IsDevelopment())
        {
            policy.AllowCredentials(); // Debug/dev only
        }
    });
});

// =========================================================================
//  configuring Serilog Logging
// =========================================================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// On remplace le logger par défaut de .NET par Serilog
builder.Host.UseSerilog();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

try
{
    Log.Information("Démarrage de l'application...");

    var app = builder.Build();

    app.UseMiddleware<ExceptionMiddleware>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        //app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
    }

    app.UseHttpsRedirection();

    // IMPORTANT : L'ordre des middlewares est critique
    app.UseRouting(); // Doit être présent pour que CORS sache quel contrôleur est visé

    //Spécifiez le nom de la police ici
    app.UseCors(AllowSpecificOriginsName);

    //Auth doit être APRES Cors
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diagContext, httpContext) =>
        {
            // On cherche d'abord le claim "sub" (Subject) qui contient ton login
            var nameClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                            ?? httpContext.User.FindFirst("sub");

            diagContext.Set("UserId", nameClaim?.Value ?? "Anonymous");
            diagContext.Set("TraceId", httpContext.TraceIdentifier);
        };

        opts.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;
    });

    //Autorise le serveur à chercher des fichiers par défaut (index.html)
    app.UseDefaultFiles();

    //Autorise le serveur à servir les fichiers physiques (JS, CSS, Images)
    app.UseStaticFiles();

    app.MapControllers();

    //LA LIGNE CRUCIALE : Si aucune route API n'est trouvée, renvoie l'index d'Angular
    app.MapFallbackToFile("index.html");

    var enableSeed = builder.Configuration.GetValue<bool>("SeedData:EnableBogus");

    if (app.Environment.IsDevelopment() && enableSeed)
    {
        Log.Information("Initialisation de la base de données de développement...");
        await app.InitialiseDbDevAsync();

    }

    // Init DB and Seed Reference Data via paramètre CLI (prod )
    if (args.Contains("--seed-reference-data"))
    {
        Console.WriteLine("Application des migrations...");
        await app.InitialiseDbProdAsync();

        Console.WriteLine("Reference data seed completed.");
        return; // termine l’exécution si c’était juste pour le seed
    }

    Log.Information("Application prête ! Lancement...");

    await app.RunAsync();
}

catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    // On ne logue l'erreur QUE si ce n'est PAS un arrêt normal d'EF Core
    Log.Fatal(ex, "L'application a réellement échoué au démarrage !");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
