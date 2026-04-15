using DotNetTemplateClean.Infrastructure;
using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests;

[SetUpFixture]
public class FunctionalTestSetup
{
    private static WebApiFactory? _factory;
    private static string? _databasePath;

    public static IServiceScopeFactory ScopeFactory { get; private set; } = null!;

    internal static DatabaseResetter? DbResetter { get; private set; }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"dotnettemplateclean-functional-tests-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={_databasePath}";

        _factory = new WebApiFactory(connectionString);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        DbResetter = await DatabaseResetter.CreateAsync(ScopeFactory);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbResetter is not null)
        {
            await DbResetter.DisposeAsync();
        }

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (!string.IsNullOrWhiteSpace(_databasePath) && File.Exists(_databasePath))
        {
            try
            {
                File.Delete(_databasePath);
            }
            catch (IOException)
            {
                // If SQLite still holds a lock briefly, keep cleanup best-effort.
            }
        }
    }
}
