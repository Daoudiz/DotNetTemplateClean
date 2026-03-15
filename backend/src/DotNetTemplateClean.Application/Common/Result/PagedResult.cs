
namespace DotNetTemplateClean.Application;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalCount { get; set; }
    
    // On ajoute cette info pour aider le Frontend
    public bool IsFullResult { get; set; }

    public PagedResult(IEnumerable<T> items, int totalCount, bool isFull = false)
    {
        Items = items;
        TotalCount = totalCount;
        IsFullResult = isFull;
    }
}
