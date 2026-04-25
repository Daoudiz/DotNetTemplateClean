using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Domain;
using DotNetTemplateClean.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Commands.CreatePersonnel;

public class CreatePersonnelFailureTests : TestBase
{
    [Test]
    public async Task ShouldRollbackPersonnelWhenIdentityCreationFails()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();

        var command = new CreatePersonnelCommand
        {
            Matricule = "777001",
            Nom = "Rollback",
            Prenom = "IdentityFailure",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = "rollback.identity@example.com",
            EntiteId = entiteId,
            Statut = "Actif",
            Grade = "Senior",
            CreateUser = true,
            UserRole = "missing-role-id",
            Affectations =
            [
                new CreateAffectationDto(
                    entiteId,
                    fonctionId,
                    new DateTime(2015, 1, 1),
                    "Titulaire")
            ]
        };

        Assert.ThrowsAsync<DomainException>(async () => await TestApp.SendAsync(command));

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var createdPersonnel = await context.Personnels
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Matricule == command.Matricule);

        Assert.That(createdPersonnel, Is.Null);
    }

    private static async Task<(int entiteId, int fonctionId)> SeedOrganizationAsync()
    {
        var typeEntite = new TypeEntite
        {
            Code = "TYPE-CREATE-FAIL",
            Libelle = "Type create fail"
        };
        await TestApp.AddAsync(typeEntite);

        var entite = new Entite
        {
            Code = "ENT-CREATE-FAIL",
            Libelle = "Entite create fail",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(entite);

        var fonction = new Fonction
        {
            Code = "FNC-CREATE-FAIL",
            Designation = "Fonction create fail",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(fonction);

        return (entite.Id, fonction.Id);
    }
}
