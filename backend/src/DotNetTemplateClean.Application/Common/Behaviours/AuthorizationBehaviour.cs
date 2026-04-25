namespace DotNetTemplateClean.Application;

public class AuthorizationBehaviour<TRequest, TResponse>(IUser user) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not IAuthorizeRequest authorizeRequest)
        {
            return await next(cancellationToken);
        }

        if (authorizeRequest.RequireAuthenticatedUser && string.IsNullOrWhiteSpace(user.Id))
        {
            throw new UnauthorizedAccessException("Authentication is required for this request.");
        }

        if (authorizeRequest.Roles.Count > 0)
        {
            var currentUserRoles = user.Roles ?? [];

            var isAuthorized = authorizeRequest.Roles.Any(requiredRole =>
                currentUserRoles.Any(currentRole =>
                    string.Equals(currentRole, requiredRole, StringComparison.OrdinalIgnoreCase)));

            if (!isAuthorized)
            {
                throw new ForbiddenAccessException();
            }
        }

        return await next(cancellationToken);
    }
}
