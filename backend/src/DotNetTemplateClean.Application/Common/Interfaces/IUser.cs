namespace DotNetTemplateClean.Application;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }

}
