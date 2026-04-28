namespace DotNetTemplateClean.Application;

public record GetEntitesByParentQuery(string TypeEntiteLibelle, int ParentId) : IRequest<List<Entite>>;

public class GetEntitesByParentQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEntitesByParentQuery, List<Entite>>
{
    public async Task<List<Entite>> Handle(GetEntitesByParentQuery request, CancellationToken cancellationToken)
    {
        return await context.Entites
            .AsNoTracking()
            .Where(e =>
                EF.Functions.Like(e.TypeEntite.Libelle, request.TypeEntiteLibelle)
                && e.RattachementEntiteId == request.ParentId)
            .Select(e => new Entite
            {
                Id = e.Id,
                Code = e.Code
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
