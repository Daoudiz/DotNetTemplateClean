namespace DotNetTemplateClean.Application;

public record GetOrganisationTreeQuery : IRequest<List<TreeNodeDto>>;

public class GetOrganisationTreeQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOrganisationTreeQuery, List<TreeNodeDto>>
{
    public async Task<List<TreeNodeDto>> Handle(GetOrganisationTreeQuery request, CancellationToken cancellationToken)
    {
        var allEntities = await context.Entites
            .AsNoTracking()
            .Select(e => new { e.Id, e.Libelle, e.RattachementEntiteId })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var nodesMap = allEntities.ToDictionary(
            e => e.Id,
            e => new TreeNodeDto
            {
                Label = e.Libelle,
                Data = e.Id
            });

        var rootNodes = new List<TreeNodeDto>();

        foreach (var entity in allEntities)
        {
            var currentNode = nodesMap[entity.Id];

            if (entity.RattachementEntiteId == null)
            {
                rootNodes.Add(currentNode);
            }
            else if (nodesMap.TryGetValue(entity.RattachementEntiteId.Value, out var parentNode))
            {
                parentNode.Children.Add(currentNode);
            }
        }

        return rootNodes;
    }
}
