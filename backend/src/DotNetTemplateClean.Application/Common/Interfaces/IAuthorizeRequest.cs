namespace DotNetTemplateClean.Application;

public interface IAuthorizeRequest
{
    bool RequireAuthenticatedUser => true;
    IReadOnlyCollection<string> Roles => [];
}
