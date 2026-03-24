using Microsoft.Extensions.Logging;

namespace DotNetTemplateClean.Application;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
   

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
#pragma warning disable CA1848 // Template should be a static expression
            logger.LogError(ex, "CleanArchitecture Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);
#pragma warning restore CA1848 // Template should be a static expression
            throw;
        }
    }
}
