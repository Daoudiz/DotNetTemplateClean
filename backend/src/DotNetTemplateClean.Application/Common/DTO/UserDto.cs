


namespace DotNetTemplateClean.Application;


public record UserCreationDto
{
    public int Matricule { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateRecrutement { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string UserRole { get; init; } = string.Empty; // L'ID du rôle
    public int Direction { get; init; }
    public int? Division { get; init; }
    public int? Service { get; init; }
    public bool TwoFactorEnabled { get; init; }
}

public record UserUpdateDto
{
    public string UserId { get; set; } = string.Empty;
    public int Matricule { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateRecrutement { get; set; }
    public int Direction { get; set; }
    public int? Division { get; set; }
    public int? Service { get; set; }
    public String UserRole { get; set; } = string.Empty;
}

public class UserSearchResultDto
{
    public required string Id { get; set; }
    public int Matricule { get; set; }
    public required string Nom { get; set; }
    public required string Prenom { get; set; }
    public DateTime? DateRecrutement { get; set; }
    public int DirectionId { get; set; }
    public int? DivisionId { get; set; }
    public int? ServiceId { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public bool IsLocked { get; set; }
    public required string Roles { get; set; } 
    public required string RoleId { get; set; }
}

public class SearchViewModel
{
    public int? Matricule { get; set; }
    public string? Nom { get; set; }
    public string? Prenom { get; set; }        
    public DateTime? DateRecrutementDebut { get; set; }
    public DateTime? DateRecrutementFin { get; set; }
    public int? DirectionId { get; set; }
    public int? DivisionId { get; set; }
    public int? ServiceId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ChangePasswordViewModel
{        
    public string OldPassword { get; set; } = String.Empty;       
    public string NewPassword { get; set; } = String.Empty;        
    public string ConfirmPassword { get; set; } = String.Empty;
}

public class ProfilViewModel
{

    public string Id { get; set; } = string.Empty;        
    public required string UserName { get; set; }        
    public string? Password { get; set; }        
    public int Matricule { get; set; }        
    public required string FirstName { get; set; }       
    public required string LastName { get; set; }       
    public DateTime? DateRecrutement { get; set; }        
    public string Email { get; set; } = string.Empty;       
    public required string Entite { get; set; }        
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
   
    public int Matricule { get; set; }  
    public required string FirstName { get; set; }      
    public required string LastName { get; set; }    
    public DateTime DateRecrutement { get; set; }    
    public int Direction { get; set; }
    public int? Division { get; set; }
    public int? Service { get; set; }   
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
    public int Matricule { get; set; }  
    public string FirstName { get; set; } = string.Empty;  
    public string LastName { get; set; } = string.Empty;   
    public string Email { get; set; } = string.Empty;
    public DateTime? DateRecrutement { get; set; }  
    public int Direction { get; set; }
    public int? Division { get; set; }
    public int? Service { get; set; }  
    public string UserRole { get; set; } = string.Empty;
}
