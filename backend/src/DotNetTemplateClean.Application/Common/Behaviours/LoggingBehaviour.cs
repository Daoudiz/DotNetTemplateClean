using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace DotNetTemplateClean.Application;

public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user, IUserService userService) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await userService.GetUserNameAsync(userId);
        }
#pragma warning disable CA1848 // Template should be a static expression
#pragma warning disable CA1873 // Template should be a static expression
        logger.LogInformation("CleanArchitecture Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
#pragma warning restore CA1873 // Template should be a static expression
#pragma warning restore CA1848 // Template should be a static expression
}
