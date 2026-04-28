namespace DotNetTemplateClean.Application;

public class EntiteHierarchyService(IApplicationDbContext context) : IEntiteHierarchyService
{
    public async Task<List<int>> GetFlattenedChildEntityIds(int parentId)
    {
        var allEntities = await context.Entites
            .AsNoTracking()
            .Select(e => new { e.Id, e.RattachementEntiteId })
            .ToListAsync()
            .ConfigureAwait(false);

        var resultIds = new HashSet<int> { parentId };
        var queue = new Queue<int>();
        queue.Enqueue(parentId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var children = allEntities
                .Where(e => e.RattachementEntiteId == currentId)
                .Select(e => e.Id);

            foreach (var childId in children)
            {
                if (resultIds.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return [.. resultIds];
    }
}
