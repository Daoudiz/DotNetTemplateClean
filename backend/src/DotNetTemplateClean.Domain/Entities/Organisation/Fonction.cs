
namespace DotNetTemplateClean.Domain;

public class Fonction : AuditableEntity<int>
{
    public required string Code { get; set; }
    public required string Designation { get; set; }
    public FonctionDomaine? Domaine { get; set; }
    public TypeFonction? Type   { get; set; }

    // The Fonction participates in AffectationPersonnel (no direct Personnel-Fonction relation)
    public ICollection<AffectationPersonnel> Affectations { get;  } = [];

    // optional link to TypeEntite if you keep that relation
    public int? TypeEntiteId { get; set; }
    public TypeEntite? TypeEntite { get; set; }
}
