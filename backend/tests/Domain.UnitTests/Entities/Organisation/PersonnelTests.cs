using DotNetTemplateClean.Domain;
using FluentAssertions;

namespace CleanArchWebApi.Tests.Entities.Organisation;

public class PersonnelTests
{
    [Test]
    public void ShouldCreatePersonnelWhenValidDataIsProvided()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1995, 3, 15));

        // Act
        var result = Personnel.Create(
            matricule: "MAT-001",
            nom: "Benali",
            prenom: "Sara",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "sara.benali@example.com",
            entiteId: 5,
            statut: "Actif",
            grade: "Senior");

        // Assert
        result.Should().NotBeNull();
        result.Matricule.Should().Be("MAT-001");
        result.Nom.Should().Be("Benali");
        result.Prenom.Should().Be("Sara");
        result.DateRecrutement.Should().Be(dateRecrutement);
        result.DateNaissance.Should().Be(dateNaissance);
        result.Email.Should().Be("sara.benali@example.com");
        result.EntiteId.Should().Be(5);
        result.Statut.Should().Be("Actif");
        result.Grade.Should().Be("Senior");
    }

    [Test]
    public void ShouldCreatePersonnelWhenAgeAtRecruitmentIsExactly18()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(2008, 1, 1));

        // Act
        var result = Personnel.Create(
            matricule: "MAT-002",
            nom: "Alaoui",
            prenom: "Nadia",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "nadia.alaoui@example.com",
            entiteId: 1,
            statut: null,
            grade: null);

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ShouldCreatePersonnelWhenAgeAtRecruitmentIsExactly45()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1981, 1, 1));

        // Act
        var result = Personnel.Create(
            matricule: "MAT-003",
            nom: "Karimi",
            prenom: "Youssef",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "youssef.karimi@example.com",
            entiteId: 2,
            statut: "Actif",
            grade: "Intermediaire");

        // Assert
        result.Should().NotBeNull();
    }

    [Test]
    public void ShouldThrowExceptionWhenMatriculeIsEmpty()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1990, 7, 10));

        // Act
        Action act = () => Personnel.Create(
            matricule: " ",
            nom: "Ben",
            prenom: "Ali",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "ben.ali@example.com",
            entiteId: 3,
            statut: null,
            grade: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*matricule*obligatoire*");
    }

    [Test]
    public void ShouldThrowExceptionWhenNomIsEmpty()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1990, 7, 10));

        // Act
        Action act = () => Personnel.Create(
            matricule: "MAT-004",
            nom: "",
            prenom: "Ali",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "ali@example.com",
            entiteId: 3,
            statut: null,
            grade: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*nom*obligatoire*");
    }

    [Test]
    public void ShouldThrowExceptionWhenPrenomIsEmpty()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1990, 7, 10));

        // Act
        Action act = () => Personnel.Create(
            matricule: "MAT-005",
            nom: "Ali",
            prenom: "",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "ali@example.com",
            entiteId: 3,
            statut: null,
            grade: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*prenom*obligatoire*");
    }

    [Test]
    public void ShouldThrowExceptionWhenDateNaissanceIsNull()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        DateNaissance dateNaissance = null!;

        // Act
        Action act = () => Personnel.Create(
            matricule: "MAT-006",
            nom: "Ait",
            prenom: "Mina",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "mina.ait@example.com",
            entiteId: 4,
            statut: null,
            grade: null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dateNaissance");
    }

    [Test]
    public void ShouldThrowExceptionWhenAgeAtRecruitmentIsLessThan18()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(2008, 1, 2));

        // Act
        Action act = () => Personnel.Create(
            matricule: "MAT-007",
            nom: "Slaoui",
            prenom: "Imane",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "imane.slaoui@example.com",
            entiteId: 6,
            statut: null,
            grade: null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*recrutement*18*45*");
    }

    [Test]
    public void ShouldThrowExceptionWhenAgeAtRecruitmentIsGreaterThan45()
    {
        // Arrange
        var dateRecrutement = new DateOnly(2026, 1, 1);
        var dateNaissance = DateNaissance.Create(new DateOnly(1979, 12, 31));

        // Act
        Action act = () => Personnel.Create(
            matricule: "MAT-008",
            nom: "Rami",
            prenom: "Lina",
            dateRecrutement: dateRecrutement,
            dateNaissance: dateNaissance,
            email: "lina.rami@example.com",
            entiteId: 7,
            statut: "Inactif",
            grade: "Junior");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*recrutement*18*45*");
    }
}
