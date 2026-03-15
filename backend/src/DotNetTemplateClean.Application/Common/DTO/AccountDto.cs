
namespace DotNetTemplateClean.Application;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public string Username { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
}

public class RegisterResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
}
