namespace DotNetTemplateClean.Application;

public record UserCreationDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string UserRole { get; init; } = string.Empty;
    public bool TwoFactorEnabled { get; init; }
}

public record UserUpdateDto
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
}

public class UserSearchResultDto
{
    public required string Id { get; set; }
    public required string Nom { get; set; }
    public required string Prenom { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public bool IsLocked { get; set; }
    public required string Roles { get; set; }
    public required string RoleId { get; set; }
}

public class SearchViewModel
{
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ChangePasswordViewModel
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfilViewModel
{
    public string Id { get; set; } = string.Empty;
    public required string UserName { get; set; }
    public string? Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public required string UserRole { get; set; }
}

public class LoginViewModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; }
}

public class CreateViewModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string UserRole { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

public class UpdateUserViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
}
