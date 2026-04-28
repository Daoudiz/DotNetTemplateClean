namespace DotNetTemplateClean.Application;

public record SearchEntitesQuery : IRequest<PagedResult<OrganizationUnitResponseDto>>
{
    public string? SearchTerm { get; init; }
    public int? TypeEntiteId { get; init; }
    public int? ParentId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 5;
}

public class SearchEntitesQueryHandler(
    IApplicationDbContext context,
    IOptions<SearchSettings> searchOptions,
    IMapper mapper,
    IEntiteHierarchyService entiteHierarchyService)
    : IRequestHandler<SearchEntitesQuery, PagedResult<OrganizationUnitResponseDto>>
{
    public async Task<PagedResult<OrganizationUnitResponseDto>> Handle(SearchEntitesQuery request, CancellationToken cancellationToken)
    {
        var query = context.Entites.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                EF.Functions.Like(x.Code!, $"%{search}%") ||
                EF.Functions.Like(x.Libelle!, $"%{search}%"));
        }

        if (request.TypeEntiteId.HasValue)
        {
            query = query.Where(x => x.TypeEntiteId == request.TypeEntiteId.Value);
        }

        if (request.ParentId.HasValue)
        {
            var allChildrenIds = await entiteHierarchyService
                .GetFlattenedChildEntityIds(request.ParentId.Value)
                .ConfigureAwait(false);

            query = query.Where(x => allChildrenIds.Contains(x.Id));
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var projection = query.OrderBy(x => x.Libelle)
            .ProjectTo<OrganizationUnitResponseDto>(mapper.ConfigurationProvider);

        if (totalCount <= searchOptions.Value.ThresholdForFullLoad)
        {
            var allItems = await projection.ToListAsync(cancellationToken).ConfigureAwait(false);
            return new PagedResult<OrganizationUnitResponseDto>(allItems, totalCount, isFull: true);
        }

        var pagedItems = await projection
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<OrganizationUnitResponseDto>(pagedItems, totalCount, isFull: false);
    }
}
