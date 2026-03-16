

namespace DotNetTemplateClean.Infrastructure;

public sealed class JwtTokenResult
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}
