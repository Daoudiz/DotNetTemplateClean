
namespace DotNetTemplateClean.Application;

public interface IApplicationDbContext
{
    DbSet<Entite> Entites { get; }
    DbSet<Fonction> Fonctions { get; }
    DbSet<AffectationPersonnel> Affectations { get;  }
    DbSet<TypeEntite> TypeEntites { get;  }
    DbSet<Personnel> Personnels { get;  }
  
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
