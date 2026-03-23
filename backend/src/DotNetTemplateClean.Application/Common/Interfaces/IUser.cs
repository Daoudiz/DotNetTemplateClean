namespace DotNetTemplateClean.Application;

public interface IUser
{
    string? Id { get; }

#pragma warning disable CA1002 
    List<string>? Roles { get; }
#pragma warning restore CA1002

}
