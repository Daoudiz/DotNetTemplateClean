


namespace DotNetTemplateClean.Application;

//TODO : Ajouter Authorize et implémnter le service de gestion des authorisations (AuthorizationBehaviour de jason)
public record GetPersonnelsWithFiltersQuery : IRequest<PaginatedList<PersonnelListDto>>
{
    public string? SearchTerm { get; init; }
    public int? EntiteId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetPersonnelsWithFiltersQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetPersonnelsWithFiltersQuery, PaginatedList<PersonnelListDto>>
{
    public async Task<PaginatedList<PersonnelListDto>> Handle(GetPersonnelsWithFiltersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
           

        var query = context.Personnels.AsNoTracking();

        // 2. Filtre par recherche textuelle (Nom, Prénom ou Matricule)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = $"%{request.SearchTerm.Trim()}%";

            query = query.Where(p =>
                EF.Functions.Like(p.Nom, term) ||
                EF.Functions.Like(p.Prenom, term) ||
                EF.Functions.Like(p.Matricule, term)
            );
        }

        // 3. Filtre par Entité (Tree List)
        // Attention : Si c'est une tree list, on filtre souvent sur l'entité actuelle 
        // ou ses enfants. Ici, on fait le cas simple de l'entité directe.
        if (request.EntiteId.HasValue)
        {
            query = query.Where(p => p.Affectations.Any(a =>
                a.EntiteId == request.EntiteId && a.DateFinAffectation == null));
        }

       

        // Projection et exécution
        return await query
                .ProjectTo<PersonnelListDto>(mapper.ConfigurationProvider)
                .OrderBy(p => p.Nom)
                .PaginatedListAsync(request.PageNumber, request.PageSize, isFull: false, cancellationToken);

       
    }
}

