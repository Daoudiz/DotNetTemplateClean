using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Domain;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Queries.GetPersonnels;

public class GetPersonnelsWithFiltersQueryTests : TestBase
{
    [Test]
    public async Task ShouldReturnPagedResultWithinBounds()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();
        await SeedPersonnelAsync(entiteId, fonctionId, "910001", "NomA");
        await SeedPersonnelAsync(entiteId, fonctionId, "910002", "NomB");
        await SeedPersonnelAsync(entiteId, fonctionId, "910003", "NomC");

        var query = new GetPersonnelsWithFiltersQuery
        {
            PageNumber = 1,
            PageSize = 2
        };

        var result = await TestApp.SendAsync(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.PageNumber, Is.EqualTo(1));
            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.TotalCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(result.TotalPages, Is.GreaterThanOrEqualTo(2));
        });
    }

    [Test]
    public async Task ShouldReturnEmptyWhenPageNumberIsOutOfBounds()
    {
        var (entiteId, fonctionId) = await SeedOrganizationAsync();
        await SeedPersonnelAsync(entiteId, fonctionId, "920001", "NomOutOfBounds");

        var query = new GetPersonnelsWithFiltersQuery
        {
            PageNumber = 999,
            PageSize = 10
        };

        var result = await TestApp.SendAsync(query);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.PageNumber, Is.EqualTo(999));
            Assert.That(result.TotalCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.TotalPages, Is.GreaterThanOrEqualTo(1));
        });
    }

    private static async Task<(int entiteId, int fonctionId)> SeedOrganizationAsync()
    {
        var typeEntite = new TypeEntite
        {
            Code = "TYPE-PAGINATION",
            Libelle = "Type pagination"
        };
        await TestApp.AddAsync(typeEntite);

        var entite = new Entite
        {
            Code = "ENT-PAGINATION",
            Libelle = "Entite pagination",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(entite);

        var fonction = new Fonction
        {
            Code = "FNC-PAGINATION",
            Designation = "Fonction pagination",
            TypeEntiteId = typeEntite.Id
        };
        await TestApp.AddAsync(fonction);

        return (entite.Id, fonction.Id);
    }

    private static async Task SeedPersonnelAsync(int entiteId, int fonctionId, string matricule, string nom)
    {
        var command = new CreatePersonnelCommand
        {
            Matricule = matricule,
            Nom = nom,
            Prenom = "Pagination",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = $"{matricule}@example.com",
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

        await TestApp.SendAsync(command);
    }
}
