

namespace DotNetTemplateClean.Infrastructure;

public interface IJwtTokenService
{
    Task<JwtTokenResult> GenerateTokenAsync(ApplicationUser user);
}
