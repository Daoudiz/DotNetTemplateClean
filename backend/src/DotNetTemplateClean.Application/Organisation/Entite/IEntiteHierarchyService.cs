namespace DotNetTemplateClean.Application;

public interface IEntiteHierarchyService
{
    Task<List<int>> GetFlattenedChildEntityIds(int parentId);
}
