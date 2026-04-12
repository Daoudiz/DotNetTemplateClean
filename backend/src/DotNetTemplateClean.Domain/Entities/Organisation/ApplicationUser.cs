
using Microsoft.AspNetCore.Identity;

namespace DotNetTemplateClean.Domain;
public class ApplicationUser : IdentityUser, IAuditableEntity
{
   
    public  string FirstName { get; set; } = string.Empty;        
    public  string LastName { get; set; }   = string.Empty;        
    public int Matricule { get; set; }
   
    public DateOnly? DateRecrutement { get; set; }
   
    public virtual int EntiteId { get; set; }
                    
    public virtual Entite Entite { get; set; } = null!;

    
    // Implémentation manuelle de l'audit (car pas d'héritage multiple possible)
    public string? CreatedBy { get; set; } 
    public DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; } 
    public DateTime? UpdatedDate { get; set; }

}
