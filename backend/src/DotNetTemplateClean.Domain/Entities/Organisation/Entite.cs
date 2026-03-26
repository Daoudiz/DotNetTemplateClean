
namespace DotNetTemplateClean.Domain;

public class Entite : AuditableEntity<int>
{              
    public required string Code { get; set; }               
    public string Libelle { get; set; } = null!;
    public int? RattachementEntiteId { get; set; }
    public virtual Entite? Rattachement { get; set; } 
    public virtual ICollection<ApplicationUser> Users { get; }  = [];
    public ICollection<Entite> Children { get; } = [];

    // Many Entite -> One TypeEntite
    public int TypeEntiteId { get; set; }
    public TypeEntite TypeEntite { get; set; } = null!;

    // N-ary association: Affectation between Personnel, Entite and Fonction
    public ICollection<AffectationPersonnel> Affectations { get;  } = [];

    public virtual ICollection<Personnel> Personnel { get; } = [];
}



