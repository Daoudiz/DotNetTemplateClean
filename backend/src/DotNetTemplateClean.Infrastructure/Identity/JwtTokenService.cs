

namespace DotNetTemplateClean.Infrastructure;


public sealed class JwtTokenService(
    IConfiguration Configuration,
    UserManager<ApplicationUser> UserManager) : IJwtTokenService
{
    public async Task<JwtTokenResult> GenerateTokenAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var jwtSection = Configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer not configured");

        var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var m)
            ? m
            : 120;

        var roles = ( await UserManager.GetRolesAsync(user)).ToArray();

        var claims = BuildClaims(user, roles);

        var signingCredentials = BuildSigningCredentials(key);

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        return new JwtTokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            Roles = roles
        };
    }

    private static IEnumerable<Claim> BuildClaims(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty);
        yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        yield return new Claim("uid", user.Id ?? string.Empty);

        foreach (var role in roles)
        {
            yield return new Claim(ClaimTypes.Role, role);
        }
    }

    private static SigningCredentials BuildSigningCredentials(string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }
}
