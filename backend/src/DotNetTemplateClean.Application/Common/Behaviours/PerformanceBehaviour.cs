using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetTemplateClean.Application;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IUserService userService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();    

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        _timer.Start();

        var response = await next(cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await userService.GetUserNameAsync(userId);
            }
#pragma warning disable CA1848 // Template should be a static expression

            logger.LogWarning("CleanArchitecture Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }
#pragma warning restore CA1848 // Template should be a static expression

        return response;
    }
}
