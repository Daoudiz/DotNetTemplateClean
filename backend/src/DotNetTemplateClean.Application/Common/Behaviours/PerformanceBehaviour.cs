using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DotNetTemplateClean.Application;

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    IUser user,
    IUserService userService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var timer = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = user.Id ?? string.Empty;
            var userName = user.Name ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userName) && !string.IsNullOrEmpty(userId))
            {
                userName = await userService.GetUserNameAsync(userId);
            }
            var correlationId = Activity.Current?.TraceId.ToString() ?? Activity.Current?.Id ?? string.Empty;
            var metadata = ExtractSafeScalarMetadata(request);
#pragma warning disable CA1848 // Template should be a static expression

            logger.LogWarning(
                "CleanArchitecture Request: {RequestName} {UserId} {UserName} {CorrelationId} {ElapsedMilliseconds}ms {@RequestMetadata}",
                requestName,
                userId,
                userName,
                correlationId,
                elapsedMilliseconds,
                metadata);
        }
#pragma warning restore CA1848 // Template should be a static expression

        return response;
    }

    private static Dictionary<string, object?> ExtractSafeScalarMetadata(TRequest request)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Take(20);

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            if (value is null)
            {
                continue;
            }

            if (!IsSafeScalar(property.PropertyType))
            {
                continue;
            }

            result[property.Name] = value is string s && s.Length > 200 ? s[..200] : value;
        }

        return result;
    }

    private static bool IsSafeScalar(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;

        return targetType.IsPrimitive
            || targetType.IsEnum
            || targetType == typeof(string)
            || targetType == typeof(decimal)
            || targetType == typeof(Guid)
            || targetType == typeof(DateTime)
            || targetType == typeof(DateTimeOffset)
            || targetType == typeof(TimeSpan)
            || targetType == typeof(DateOnly)
            || targetType == typeof(TimeOnly);
    }
}
