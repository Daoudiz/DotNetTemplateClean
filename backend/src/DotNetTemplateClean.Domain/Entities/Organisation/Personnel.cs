
namespace DotNetTemplateClean.Domain;

public class Personnel : AuditableEntity<int>
{
    public required string Matricule { get; set; }
    public required string Nom { get; set; }
    public required string Prenom { get; set; }
    public DateTime? DateRecrutement { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? Statut { get; set; }
    public string? Grade { get; set; }
    // optional link to ApplicationUser
    public string? IdentityId { get; set; }   

    // N-ary association: Affectation between Personnel, Entite and Fonction
    public ICollection<AffectationPersonnel> Affectations { get;  } = [];
}
