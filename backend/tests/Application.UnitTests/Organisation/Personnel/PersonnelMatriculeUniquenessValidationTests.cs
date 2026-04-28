using FluentAssertions;

using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

namespace DotNetTemplateClean.UnitTest;

[TestFixture]
public class PersonnelMatriculeUniquenessValidationTests
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<TestApplicationDbContext> _dbOptions = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _dbOptions = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new TestApplicationDbContext(_dbOptions);
        context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
    }

    [Test]
    public async Task Create_Should_Fail_When_Matricule_Already_Exists()
    {
        await using var context = new TestApplicationDbContext(_dbOptions);
        await EnsureEntiteExistsAsync(context, 10);
        context.Personnels.Add(CreatePersonnelEntity(1, "MAT-001", 10));
        await context.SaveChangesAsync();

        var uniquenessService = new PersonnelMatriculeUniquenessService(context);
        var validator = new CreatePersonnelCommandValidator(uniquenessService);

        var command = CreateValidCreateCommand("MAT-001", 10);
        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreatePersonnelCommand.Matricule)
            && e.ErrorMessage == "Ce matricule est deja utilise par un autre personnel.");
    }

    [Test]
    public async Task Update_Should_Succeed_When_Keeping_Same_Matricule()
    {
        await using var context = new TestApplicationDbContext(_dbOptions);
        await EnsureEntiteExistsAsync(context, 10);
        context.Personnels.Add(CreatePersonnelEntity(1, "MAT-001", 10));
        await context.SaveChangesAsync();

        var uniquenessService = new PersonnelMatriculeUniquenessService(context);
        var validator = new UpdatePersonnelCommandValidator(context, uniquenessService);

        var command = CreateValidUpdateCommand(1, "MAT-001", 10);
        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(UpdatePersonnelCommand.Matricule));
    }

    [Test]
    public async Task Update_Should_Fail_When_Matricule_Belongs_To_Another_Personnel()
    {
        await using var context = new TestApplicationDbContext(_dbOptions);
        await EnsureEntiteExistsAsync(context, 10);
        context.Personnels.Add(CreatePersonnelEntity(1, "MAT-001", 10));
        context.Personnels.Add(CreatePersonnelEntity(2, "MAT-002", 10));
        await context.SaveChangesAsync();

        var uniquenessService = new PersonnelMatriculeUniquenessService(context);
        var validator = new UpdatePersonnelCommandValidator(context, uniquenessService);

        var command = CreateValidUpdateCommand(1, "MAT-002", 10);
        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(UpdatePersonnelCommand.Matricule)
            && e.ErrorMessage == "Ce matricule est deja utilise par un autre personnel.");
    }

    private static CreatePersonnelCommand CreateValidCreateCommand(string matricule, int entiteId)
        => new()
        {
            Matricule = matricule,
            Nom = "Nom",
            Prenom = "Prenom",
            DateRecrutement = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-1)),
            DateNaissance = DateTime.UtcNow.Date.AddYears(-25),
            EntiteId = entiteId,
            Email = "personnel@example.com",
            Affectations =
            [
                new CreateAffectationDto(entiteId, 1, DateTime.UtcNow.Date, "Initiale")
            ]
        };

    private static UpdatePersonnelCommand CreateValidUpdateCommand(int id, string matricule, int entiteId)
        => new()
        {
            Id = id,
            Matricule = matricule,
            Nom = "Nom",
            Prenom = "Prenom",
            DateRecrutement = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-1)),
            Affectations =
            [
                new UpdateAffectationDto(0, entiteId, 1, DateTime.UtcNow.Date, "Initiale", null)
            ]
        };

    private static Personnel CreatePersonnelEntity(int id, string matricule, int entiteId)
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-25));
        var recruitmentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-1));

        var personnel = Personnel.Create(
            matricule,
            "Nom",
            "Prenom",
            recruitmentDate,
            DateNaissance.Create(birthDate),
            "personnel@example.com",
            entiteId,
            null,
            null);

        personnel.Id = id;
        return personnel;
    }

    private static async Task EnsureEntiteExistsAsync(TestApplicationDbContext context, int entiteId)
    {
        if (!await context.TypeEntites.AnyAsync())
        {
            context.TypeEntites.Add(new TypeEntite
            {
                Id = 1,
                Code = "TYPE-1",
                Libelle = "Type 1"
            });
        }

        if (!await context.Entites.AnyAsync(e => e.Id == entiteId))
        {
            context.Entites.Add(new Entite
            {
                Id = entiteId,
                Code = $"ENT-{entiteId}",
                Libelle = $"Entite {entiteId}",
                TypeEntiteId = 1
            });
        }

        await context.SaveChangesAsync();
    }

    private sealed class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Entite> Entites => Set<Entite>();
        public DbSet<Fonction> Fonctions => Set<Fonction>();
        public DbSet<AffectationPersonnel> AffectationsPersonnel => Set<AffectationPersonnel>();
        public DbSet<TypeEntite> TypeEntites => Set<TypeEntite>();
        public DbSet<Personnel> Personnels => Set<Personnel>();

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            await action();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Personnel>(builder =>
            {
                builder.Ignore(p => p.DateNaissance);
                builder.Ignore(p => p.Entite);
                builder.Ignore(p => p.Affectations);
                builder.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Personnel>()
                .HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
