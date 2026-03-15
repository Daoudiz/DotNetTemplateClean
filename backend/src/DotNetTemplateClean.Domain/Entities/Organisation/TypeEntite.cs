namespace DotNetTemplateClean.Domain;

public class TypeEntite : AuditableEntity<int>
{
    public required string Code { get; set; }
    public required string Libelle { get; set; }
    public int? Rang { get; set; }

    public ICollection<Entite> Entites { get; } = [];
    public ICollection<Fonction> Fonctions { get; } = [];
}
