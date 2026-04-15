using DotNetTemplateClean.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Infrastructure;

internal sealed class DatabaseResetter : IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;

    private DatabaseResetter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public static Task<DatabaseResetter> CreateAsync(IServiceScopeFactory scopeFactory)
    {
        return Task.FromResult(new DatabaseResetter(scopeFactory));
    }

    public async Task ResetAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
