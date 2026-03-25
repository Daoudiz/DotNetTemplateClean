
namespace DotNetTemplateClean.Application;

public record CreatePersonnelCommand : IRequest<int>
{
    public required string Matricule { get; init; }
    public required string Nom { get; init; }
    public required string Prenom { get; init; }
    public DateTime? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string? Statut { get; init; }
    public string? Grade { get; init; }   

    // Liste des affectations initiales
    public IList<CreateAffectationDto> Affectations { get; init; } = [];
}

public record CreateAffectationDto(int EntiteId, int FonctionId, DateTime DateDebut, string Nature);

public class CreatePersonnelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreatePersonnelCommand, int>
{
    public async Task<int> Handle(CreatePersonnelCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        //Instanciation de l'entité Personnel
        var entity = new Personnel
        {
            Matricule = request.Matricule,
            Nom = request.Nom,
            Prenom = request.Prenom,
            DateRecrutement = request.DateRecrutement,
            DateNaissance = request.DateNaissance,
            Statut = request.Statut,
            Grade = request.Grade,            
        };

        // Création des affectations liées
        foreach (var aff in request.Affectations)
        {
            entity.Affectations.Add(new AffectationPersonnel
            {
                EntiteId = aff.EntiteId,
                FonctionId = aff.FonctionId,
                DateDebutAffectation = aff.DateDebut,
                Nature = aff.Nature
                // Le PersonnelId sera injecté automatiquement par EF Core lors du Save
            });
        }

        context.Personnels.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
