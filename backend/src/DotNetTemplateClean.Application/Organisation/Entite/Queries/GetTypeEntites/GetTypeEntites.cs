namespace DotNetTemplateClean.Application;

public record GetTypeEntitesQuery : IRequest<List<TypeEntiteDto>>;

public class GetTypeEntitesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTypeEntitesQuery, List<TypeEntiteDto>>
{
    public async Task<List<TypeEntiteDto>> Handle(GetTypeEntitesQuery request, CancellationToken cancellationToken)
    {
        return await context.TypeEntites
            .AsNoTracking()
            .OrderBy(t => t.Rang)
            .Select(e => new TypeEntiteDto
            {
                Id = e.Id,
                Libelle = e.Libelle ?? "Sans libelle"
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
