
namespace DotNetTemplateClean.Application;

public class PagedResult<T>(IEnumerable<T> items, int totalCount, bool isFull = false)
{
    public IEnumerable<T> Items { get; set; } = items;
    public int TotalCount { get; set; } = totalCount;

    // True si le résultat contient tous les éléments (pas de pagination), false si c'est une page partielle
    public bool IsFullResult { get; set; } = isFull;
}
