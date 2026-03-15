
namespace DotNetTemplateClean.Domain;

public class Fonction : AuditableEntity<int>
{
    public required string Code { get; set; }
    public required string Designation { get; set; }
    public string? Domaine { get; set; }
    public string? Type   { get; set; }

    // The Fonction participates in AffectationPersonnel (no direct Personnel-Fonction relation)
    public ICollection<AffectationPersonnel> Affectations { get;  } = [];

    // optional link to TypeEntite if you keep that relation
    public int? TypeEntiteId { get; set; }
    public TypeEntite? TypeEntite { get; set; }
}
