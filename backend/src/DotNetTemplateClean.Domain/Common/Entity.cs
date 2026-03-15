namespace DotNetTemplateClean.Domain;

public abstract class BaseEntity : ISoftDelete
{
    public bool IsDeleted { get; set; }
}

public abstract class Entity<T> : BaseEntity, IEntity<T>
{
    public virtual T Id { get; set; } = default!;
}
