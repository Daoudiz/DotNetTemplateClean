using DotNetTemplateClean.Domain;
using DotNetTemplateClean.Infrastructure;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Infrastructure;

public static class TestApp
{
    private static string? _userId;
    private static List<string>? _roles;

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(request);
    }

    public static string? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", []);
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", ["Admin"]);
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        ArgumentNullException.ThrowIfNull(roles);

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var entiteId = await EnsureEntiteIdAsync(context);

        var existingUser = await userManager.FindByNameAsync(userName);
        if (existingUser is not null)
        {
            _userId = existingUser.Id;
            _roles = [.. roles];
            return _userId;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
            FirstName = "Test",
            LastName = "User",
            Matricule = 999999,
            EntiteId = entiteId,
            DateRecrutement = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Unable to create {userName}.{Environment.NewLine}{errors}");
        }

        _userId = user.Id;
        _roles = new List<string>(roles);
        return _userId;
    }

    private static async Task<int> EnsureEntiteIdAsync(ApplicationDbContext context)
    {
        var existingEntiteId = await context.Entites
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        if (existingEntiteId.HasValue)
        {
            return existingEntiteId.Value;
        }

        var typeEntiteId = await context.TypeEntites
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        if (!typeEntiteId.HasValue)
        {
            var typeEntite = new TypeEntite
            {
                Code = "TST-TYPE",
                Libelle = "Type test setup"
            };

            context.TypeEntites.Add(typeEntite);
            await context.SaveChangesAsync();
            typeEntiteId = typeEntite.Id;
        }

        var entite = new Entite
        {
            Code = "TST-ENTITE",
            Libelle = "Entite test setup",
            TypeEntiteId = typeEntiteId.Value
        };

        context.Entites.Add(entite);
        await context.SaveChangesAsync();

        return entite.Id;
    }

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);
        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>()
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}
