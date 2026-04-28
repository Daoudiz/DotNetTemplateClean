namespace DotNetTemplateClean.Application;

public record GetEntitesByTypeQuery(string TypeEntiteLibelle, bool SelectIdAndCodeOnly = false) : IRequest<List<Entite>>;

public class GetEntitesByTypeQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetEntitesByTypeQuery, List<Entite>>
{
    public async Task<List<Entite>> Handle(GetEntitesByTypeQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = context.Entites
            .AsNoTracking()
            .Where(e => EF.Functions.Like(e.TypeEntite.Libelle, request.TypeEntiteLibelle));

        if (request.SelectIdAndCodeOnly)
        {
            return await baseQuery
                .Select(e => new Entite
                {
                    Id = e.Id,
                    Code = e.Code
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        return await baseQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
