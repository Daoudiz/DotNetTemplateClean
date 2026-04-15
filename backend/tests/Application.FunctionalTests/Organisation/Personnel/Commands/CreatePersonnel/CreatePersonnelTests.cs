using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Domain;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Commands.CreatePersonnel;

public class CreatePersonnelTests : TestBase
{
    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new CreatePersonnelCommand
        {
            Matricule = string.Empty,
            Nom = string.Empty,
            Prenom = string.Empty,
            DateNaissance = DateTime.UtcNow.AddYears(-20),
            DateRecrutement = null,
            EntiteId = 1,
            Affectations = []
        };

        async Task action() => await TestApp.SendAsync(command);

        Assert.ThrowsAsync<ValidationException>(action);
    }

    [Test]
    public void ShouldRequireUniqueMatricule()
    {
        async Task action()
        {
            var typeEntite = new TypeEntite
            {
                Code = "TYPE-01",
                Libelle = "Type test"
            };

            await TestApp.AddAsync(typeEntite);

            var entite = new Entite
            {
                Code = "ENT-001",
                Libelle = "Entite test",
                TypeEntiteId = typeEntite.Id
            };

            await TestApp.AddAsync(entite);

            var fonction = new Fonction
            {
                Code = "FNC-001",
                Designation = "Fonction test",
                TypeEntiteId = typeEntite.Id
            };

            await TestApp.AddAsync(fonction);

            var firstCommand = new CreatePersonnelCommand
            {
                Matricule = "123456",
                Nom = "Doe",
                Prenom = "John",
                DateNaissance = new DateTime(1990, 1, 1),
                DateRecrutement = new DateOnly(2015, 1, 1),
                Email = "john.doe@example.com",
                EntiteId = entite.Id,
                Statut = "Actif",
                Grade = "Senior",
                CreateUser = false,
                Affectations =
                [
                    new CreateAffectationDto(
                        entite.Id,
                        fonction.Id,
                        new DateTime(2015, 1, 1),
                        "Titulaire")
                ]
            };

            await TestApp.SendAsync(firstCommand);

            var duplicateMatriculeCommand = new CreatePersonnelCommand
            {
                Matricule = firstCommand.Matricule,
                Nom = "Smith",
                Prenom = "Jane",
                DateNaissance = new DateTime(1992, 2, 2),
                DateRecrutement = new DateOnly(2016, 2, 2),
                Email = "jane.smith@example.com",
                EntiteId = entite.Id,
                Statut = "Actif",
                Grade = "Junior",
                CreateUser = false,
                Affectations =
                [
                    new CreateAffectationDto(
                        entite.Id,
                        fonction.Id,
                        new DateTime(2016, 2, 2),
                        "Titulaire")
                ]
            };

            await TestApp.SendAsync(duplicateMatriculeCommand);
        }

        Assert.ThrowsAsync<ValidationException>(action);
    }

    [Test]
    public async Task ShouldCreatePersonnel()
    {
        var typeEntite = new TypeEntite
        {
            Code = "TYPE-01",
            Libelle = "Type test"
        };

        await TestApp.AddAsync(typeEntite);

        var entite = new Entite
        {
            Code = "ENT-001",
            Libelle = "Entite test",
            TypeEntiteId = typeEntite.Id
        };

        await TestApp.AddAsync(entite);

        var fonction = new Fonction
        {
            Code = "FNC-001",
            Designation = "Fonction test",
            TypeEntiteId = typeEntite.Id
        };

        await TestApp.AddAsync(fonction);

        var command = new CreatePersonnelCommand
        {
            Matricule = "123456",
            Nom = "Doe",
            Prenom = "John",
            DateNaissance = new DateTime(1990, 1, 1),
            DateRecrutement = new DateOnly(2015, 1, 1),
            Email = "john.doe@example.com",
            EntiteId = entite.Id,
            Statut = "Actif",
            Grade = "Senior",
            CreateUser = false,
            Affectations =
            [
                new CreateAffectationDto(
                    entite.Id,
                    fonction.Id,
                    new DateTime(2015, 1, 1),
                    "Titulaire")
            ]
        };

        var personnelId = await TestApp.SendAsync(command);

        var personnel = await TestApp.FindAsync<DotNetTemplateClean.Domain.Personnel>(personnelId);

        Assert.Multiple(() =>
        {
            Assert.That(personnel, Is.Not.Null);
            Assert.That(personnel!.Matricule, Is.EqualTo(command.Matricule));
            Assert.That(personnel.Nom, Is.EqualTo(command.Nom));
            Assert.That(personnel.Prenom, Is.EqualTo(command.Prenom));
            Assert.That(personnel.EntiteId, Is.EqualTo(entite.Id));
            Assert.That(personnel.IdentityId, Is.Null);
        });

        var affectationCount = await TestApp.CountAsync<AffectationPersonnel>();
        Assert.That(affectationCount, Is.EqualTo(1));
    }
}
