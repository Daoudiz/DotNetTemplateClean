using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Domain;
using DotNetTemplateClean.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Commands.CreatePersonnel;

public class CreatePersonnelTemporaryPasswordTests : TestBase
{
    [Test]
    public async Task ShouldGenerateStrongTemporaryPasswordAndRequirePasswordChangeOnFirstLogin()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();
        var roleId = await EnsureAdminRoleAsync();

        var command = new CreatePersonnelCommand
        {
            Matricule = "TMP-1001",
            Nom = "SecureNom",
            Prenom = "SecurePrenom",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = "tmp-user-1001@example.com",
            EntiteId = entiteId,
            Statut = "Actif",
            Grade = "Senior",
            CreateUser = true,
            UserRole = roleId,
            Affectations =
            [
                new CreateAffectationDto(
                    entiteId,
                    fonctionId,
                    new DateTime(2015, 1, 1),
                    "Titulaire")
            ]
        };

        var result = await TestApp.SendAsync(command);

        Assert.That(result.PersonnelId, Is.GreaterThan(0));
        Assert.That(result.TemporaryPassword, Is.Not.Null.And.Not.Empty);

        var temporaryPassword = result.TemporaryPassword!;
        Assert.Multiple(() =>
        {
            Assert.That(temporaryPassword, Is.Not.EqualTo($"{command.Prenom}@2026"));
            Assert.That(temporaryPassword.Length, Is.GreaterThanOrEqualTo(12));
            Assert.That(temporaryPassword, Does.Not.Contain(command.Prenom).IgnoreCase);
            Assert.That(temporaryPassword, Does.Not.Contain(command.Nom).IgnoreCase);
            Assert.That(temporaryPassword, Does.Not.Contain(command.Matricule).IgnoreCase);
            Assert.That(temporaryPassword.Any(char.IsUpper), Is.True);
            Assert.That(temporaryPassword.Any(char.IsLower), Is.True);
            Assert.That(temporaryPassword.Any(char.IsDigit), Is.True);
            Assert.That(temporaryPassword.Any(ch => !char.IsLetterOrDigit(ch)), Is.True);
        });

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var createdUser = await context.Users.SingleAsync(x => x.Email == command.Email);
        Assert.Multiple(() =>
        {
            Assert.That(createdUser.MustChangePassword, Is.True);
            Assert.That(createdUser.PasswordHash, Is.Not.Null.And.Not.Empty);
            Assert.That(createdUser.PasswordHash, Does.Not.Contain(temporaryPassword));
        });

        var loginResult = await userService.LoginAsync(new LoginViewModel
        {
            UserName = command.Email,
            Password = temporaryPassword
        });

        Assert.Multiple(() =>
        {
            Assert.That(loginResult.IsSuccess, Is.False);
            Assert.That(loginResult.StatusCode, Is.EqualTo(403));
            Assert.That(loginResult.ErrorMessage, Does.Contain("Changement de mot de passe requis").IgnoreCase);
        });
    }

    [Test]
    public async Task ShouldReturnDifferentTemporaryPasswordsForDifferentAutoCreatedUsers()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();
        var roleId = await EnsureAdminRoleAsync();

        var first = await TestApp.SendAsync(BuildCommand("TMP-2001", "tmp-user-2001@example.com", entiteId, fonctionId, roleId));
        var second = await TestApp.SendAsync(BuildCommand("TMP-2002", "tmp-user-2002@example.com", entiteId, fonctionId, roleId));

        Assert.That(first.TemporaryPassword, Is.Not.EqualTo(second.TemporaryPassword));
    }

    private static CreatePersonnelCommand BuildCommand(string matricule, string email, int entiteId, int fonctionId, string roleId)
        => new()
        {
            Matricule = matricule,
            Nom = "Nom",
            Prenom = "Prenom",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = email,
            EntiteId = entiteId,
            Statut = "Actif",
            Grade = "Senior",
            CreateUser = true,
            UserRole = roleId,
            Affectations =
            [
                new CreateAffectationDto(
                    entiteId,
                    fonctionId,
                    new DateTime(2015, 1, 1),
                    "Titulaire")
            ]
        };

    private static async Task<string> EnsureAdminRoleAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var existing = await roleManager.FindByNameAsync("Admin");
        if (existing is not null)
        {
            return existing.Id;
        }

        var role = new IdentityRole("Admin");
        var createResult = await roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Unable to create Admin role for tests.");
        }

        return role.Id;
    }

    private static async Task<(int entiteId, int fonctionId)> SeedOrganizationAsync()
    {
        var typeEntite = new TypeEntite
        {
            Code = $"TYPE-TEMP-PWD-{Guid.NewGuid():N}",
            Libelle = "Type temp pwd"
        };
        await TestApp.AddAsync(typeEntite);

        var entite = new Entite
        {
            Code = $"ENT-TEMP-PWD-{Guid.NewGuid():N}",
            Libelle = "Entite temp pwd",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(entite);

        var fonction = new Fonction
        {
            Code = $"FNC-TEMP-PWD-{Guid.NewGuid():N}",
            Designation = "Fonction temp pwd",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(fonction);

        return (entite.Id, fonction.Id);
    }
}
