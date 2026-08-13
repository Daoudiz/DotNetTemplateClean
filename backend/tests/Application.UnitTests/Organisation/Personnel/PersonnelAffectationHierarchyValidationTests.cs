using FluentAssertions;

using DotNetTemplateClean.Application;
using DotNetTemplateClean.Domain;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

namespace DotNetTemplateClean.UnitTest;

[TestFixture]
public sealed class PersonnelAffectationHierarchyValidationTests : IDisposable
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
    public void TearDown() => _connection.Dispose();

    [Test]
    public async Task CreateSucceedsWithAffectationInIndirectDescendant()
    {
        await using var context = new TestApplicationDbContext(_dbOptions);
        await AddHierarchyAsync(context);
        var validator = CreateValidator(context);
        var command = CreateValidCommand(rattachementEntiteId: 10, affectationEntiteId: 12);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task CreateFailsWithAffectationOutsideRattachementHierarchy()
    {
        await using var context = new TestApplicationDbContext(_dbOptions);
        await AddHierarchyAsync(context);
        var validator = CreateValidator(context);
        var command = CreateValidCommand(rattachementEntiteId: 10, affectationEntiteId: 20);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error =>
            error.ErrorMessage == PersonnelAffectationValidationExtensions
                .MissingActiveRattachementHierarchyAffectationMessage);
    }

    private static CreatePersonnelCommandValidator CreateValidator(TestApplicationDbContext context)
    {
        var uniquenessService = new PersonnelMatriculeUniquenessService(context);
        var hierarchyService = new EntiteHierarchyService(context);
        return new CreatePersonnelCommandValidator(uniquenessService, hierarchyService);
    }

    private static CreatePersonnelCommand CreateValidCommand(int rattachementEntiteId, int affectationEntiteId)
        => new()
        {
            Matricule = "MAT-001",
            Nom = "Nom",
            Prenom = "Prenom",
            DateRecrutement = new DateOnly(2025, 1, 1),
            DateNaissance = new DateTime(1990, 1, 1),
            EntiteId = rattachementEntiteId,
            Email = "personnel@example.com",
            Affectations =
            [
                new CreateAffectationDto(
                    affectationEntiteId,
                    1,
                    new DateTime(2025, 1, 1),
                    "Initiale")
            ]
        };

    private static async Task AddHierarchyAsync(TestApplicationDbContext context)
    {
        context.TypeEntites.Add(new TypeEntite
        {
            Id = 1,
            Code = "TYPE-1",
            Libelle = "Type 1"
        });

        context.Entites.AddRange(
            CreateEntite(10),
            CreateEntite(11, 10),
            CreateEntite(12, 11),
            CreateEntite(20));

        await context.SaveChangesAsync();
    }

    private static Entite CreateEntite(int id, int? rattachementEntiteId = null)
        => new()
        {
            Id = id,
            Code = $"ENT-{id}",
            Libelle = $"Entite {id}",
            TypeEntiteId = 1,
            RattachementEntiteId = rattachementEntiteId
        };

    public void Dispose() => _connection?.Dispose();

    private sealed class TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Entite> Entites => Set<Entite>();
        public DbSet<Fonction> Fonctions => Set<Fonction>();
        public DbSet<AffectationPersonnel> AffectationsPersonnel => Set<AffectationPersonnel>();
        public DbSet<TypeEntite> TypeEntites => Set<TypeEntite>();
        public DbSet<Personnel> Personnels => Set<Personnel>();

        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken) => await action();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Personnel>(builder =>
            {
                builder.Ignore(personnel => personnel.DateNaissance);
                builder.Ignore(personnel => personnel.Entite);
                builder.Ignore(personnel => personnel.Affectations);
                builder.HasKey(personnel => personnel.Id);
            });
        }
    }
}
