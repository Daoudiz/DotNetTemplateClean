
namespace DotNetTemplateClean.Application;

public record GetFonctionsTreeQuery : IRequest<List<PrimeNgTreeNodeDto>>;

public class GetFonctionsTreeQueryHandler(IApplicationDbContext context) : IRequestHandler<GetFonctionsTreeQuery, List<PrimeNgTreeNodeDto>>
{
    public async Task<List<PrimeNgTreeNodeDto>> Handle(GetFonctionsTreeQuery request, CancellationToken cancellationToken)
    {
        var fonctions = await context.Fonctions
            .AsNoTracking()
            .OrderBy(f => f.Domaine)
            .ThenBy(f => f.Designation)
            .ToListAsync(cancellationToken);

        // Build PrimeNG p-treeselect compatible tree
        var tree = fonctions
            .GroupBy(f => f.Domaine)
            .OrderBy(g => g.Key)
            .Select(g => new PrimeNgTreeNodeDto(
                Key: g.Key.HasValue ? $"domaine-{g.Key.Value}" : "domaine-unspecified",
                Label: g.Key.HasValue ? g.Key.Value.GetDisplayName() : "Unspecified",
                Data: null,
                Children: [.. g.Select(f => new PrimeNgTreeNodeDto(
                    Key: $"fonction-{f.Id}",
                    Label: $"{f.Designation} ({f.Code})",
                    Data: new FonctionNodeData(f.Id, f.Code, f.Designation, f.Type.HasValue ? f.Type.Value.ToString() : null),
                    Children: [],
                    Selectable: true
                )).OrderBy(f => f.Label)],
                    ExpandedIcon: "pi pi-folder-open",
                    CollapsedIcon: "pi pi-folder",
                    Selectable: false
            )).ToList();

        return tree;
    }
}
