using Ardalis.GuardClauses;

namespace DotNetTemplateClean.Application;

public record UpdatePersonnelCommand : IRequest
{
    public required int Id { get; init; }
    public required string Matricule { get; init; }
    public required string Nom { get; init; }
    public required string Prenom { get; init; }
    public DateTime? DateRecrutement { get; init; }
    public DateTime? DateNaissance { get; init; }
    public string? Statut { get; init; }
    public string? Grade { get; init; }

    // Liste des affectations initiales
    public IList<UpdateAffectationDto> Affectations { get; init; } = [];
}

public record UpdateAffectationDto(int Id, int EntiteId, int FonctionId, DateTime DateDebut, string Nature, DateTime? DateFinAffectation);

public class UpdatePersonnelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdatePersonnelCommand>
{
    public async Task Handle(UpdatePersonnelCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var updatedPersonnel = await context.Personnels
            .Include(x => x.Affectations)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, updatedPersonnel);

        // Mise a jour des proprietes
        updatedPersonnel.Matricule = request.Matricule;
        updatedPersonnel.Nom = request.Nom;
        updatedPersonnel.Prenom = request.Prenom;
        updatedPersonnel.DateRecrutement = request.DateRecrutement;
        updatedPersonnel.DateNaissance = request.DateNaissance;
        updatedPersonnel.Statut = request.Statut;
        updatedPersonnel.Grade = request.Grade;

        var existing = updatedPersonnel.Affectations.ToList();

        // UPDATE + ADD des affectations fournies dans la requete
        foreach (var aff in request.Affectations)
        {
            var existingAff = existing.FirstOrDefault(a => a.Id == aff.Id);

            if (existingAff != null)
            {
                // UPDATE
                existingAff.EntiteId = aff.EntiteId;
                existingAff.FonctionId = aff.FonctionId;
                existingAff.DateDebutAffectation = aff.DateDebut;
                existingAff.Nature = aff.Nature;
                existingAff.DateFinAffectation = aff.DateFinAffectation;

                if (aff.DateFinAffectation.HasValue)
                {
                    existingAff.IsActive = false;
                }
                else
                {
                    existingAff.IsActive = true;
                }
            }
            else
            {
                // ADD
                updatedPersonnel.Affectations.Add(new AffectationPersonnel
                {
                    EntiteId = aff.EntiteId,
                    FonctionId = aff.FonctionId,
                    DateDebutAffectation = aff.DateDebut,
                    Nature = aff.Nature,
                    DateFinAffectation = aff.DateFinAffectation,
                    IsActive = !aff.DateFinAffectation.HasValue
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
