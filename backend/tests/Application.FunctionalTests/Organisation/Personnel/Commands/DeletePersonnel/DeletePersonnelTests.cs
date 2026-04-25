using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Domain;
using DotNetTemplateClean.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Commands.DeletePersonnel;

public class DeletePersonnelTests : TestBase
{
    [Test]
    public async Task ShouldSoftDeletePersonnelAndAffectations()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();

        var createCommand = new CreatePersonnelCommand
        {
            Matricule = "888001",
            Nom = "Delete",
            Prenom = "Soft",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = "delete.soft@example.com",
            EntiteId = entiteId,
            Statut = "Actif",
            Grade = "Senior",
            CreateUser = false,
            Affectations =
            [
                new CreateAffectationDto(
                    entiteId,
                    fonctionId,
                    new DateTime(2015, 1, 1),
                    "Titulaire")
            ]
        };

        var personnelId = await TestApp.SendAsync(createCommand);
        await TestApp.SendAsync(new DeletePersonnelCommand(personnelId));

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var visiblePersonnel = await context.Personnels
            .SingleOrDefaultAsync(x => x.Id == personnelId);

        Assert.That(visiblePersonnel, Is.Null);

        var deletedPersonnel = await context.Personnels
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == personnelId);

        var deletedAffectations = await context.AffectationsPersonnel
            .IgnoreQueryFilters()
            .Where(x => x.PersonnelId == personnelId)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(deletedPersonnel.IsDeleted, Is.True);
            Assert.That(deletedAffectations.Count, Is.EqualTo(1));
            Assert.That(deletedAffectations.All(x => x.IsDeleted), Is.True);
        });
    }

    private static async Task<(int entiteId, int fonctionId)> SeedOrganizationAsync()
    {
        var typeEntite = new TypeEntite
        {
            Code = "TYPE-DELETE",
            Libelle = "Type delete"
        };
        await TestApp.AddAsync(typeEntite);

        var entite = new Entite
        {
            Code = "ENT-DELETE",
            Libelle = "Entite delete",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(entite);

        var fonction = new Fonction
        {
            Code = "FNC-DELETE",
            Designation = "Fonction delete",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(fonction);

        return (entite.Id, fonction.Id);
    }
}
