
using System.Linq.Expressions;

namespace DotNetTemplateClean.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                  IUser user) : IdentityDbContext<ApplicationUser>(options) , IApplicationDbContext
{

    public DbSet<Entite> Entites { get; set; }
    public DbSet<Fonction> Fonctions { get; set; }
    public DbSet<AffectationPersonnel> AffectationsPersonnel { get; set; }
    public DbSet<TypeEntite> TypeEntites { get; set; }
    public DbSet<Personnel> Personnels { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        //Configurer Identity en premier
        base.OnModelCreating(builder);        

        var identityTypes = new[]
        {
            typeof(IdentityUser),
            typeof(IdentityRole),
            typeof(IdentityUserRole<string>),
            typeof(IdentityUserClaim<string>),
            typeof(IdentityUserLogin<string>),
            typeof(IdentityUserToken<string>),
            typeof(IdentityRoleClaim<string>)
        };


        //Comportement Global : Restreindre la suppression en cascade 
        // On l'applique sur toutes les entités sauf celles d'Identity pour éviter les conflits
        foreach (var foreignKey in builder.Model.GetEntityTypes()
                    .Where(e => e.ClrType != null &&
                                !identityTypes.Any(t => t.IsAssignableFrom(e.ClrType)))
                    .SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }


        //Comportement Global: Filtre automatique pour le Soft Delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // On vérifie si l'entité implémente ISoftDelete
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                // On configure le filtre e.IsDeleted == false
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);

            }
        }

        // Scanne l'assembly actuel et applique TOUTES les classes 
        // qui implémentent IEntityTypeConfiguration<T> pour la config Fluent API des relations , contraintes, etc.
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);   
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

       //var User = _httpContextAccessor?.HttpContext?.User;
       // var identityName = User?.Identity?.Name
       //        ?? User?.FindFirst("name")?.Value
       //        ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
       //        ?? "System";
        //var identityName = User.Identity?.Name ?? "System";
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            var entity = (IAuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedBy = user.Id;
                entity.CreatedDate = now;
            }
            else
            {
                // On verrouille les champs de création pour qu'ils ne changent jamais
                entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreatedDate)).IsModified = false;
            }

            entity.UpdatedBy = user.Id;
            entity.UpdatedDate = now;
        }
    }

    // Dans ton projet INFRASTRUCTURE
    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
