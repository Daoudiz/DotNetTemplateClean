
namespace DotNetTemplateClean.Application;

public interface IApplicationDbContext
{
    DbSet<Entite> Entites { get; }
    DbSet<Fonction> Fonctions { get; }
    DbSet<AffectationPersonnel> AffectationsPersonnel { get;  }
    DbSet<TypeEntite> TypeEntites { get;  }
    DbSet<Personnel> Personnels { get;  }

    // Abstractions pour la transaction
    // Une seule méthode pour tout gérer proprement
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
