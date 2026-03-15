
namespace DotNetTemplateClean.Domain;


public class AffectationPersonnel : AuditableEntity<int>
{
    // Clés étrangéres vers les entités liées
    public int PersonnelId { get; set; }
    public Personnel Personnel { get; set; } = null!;

    public int EntiteId { get; set; }
    public Entite Entite { get; set; } = null!;

    public int FonctionId { get; set; }
    public Fonction Fonction { get; set; } = null!;

    // champs supplémentaires pour gérer les affectations
    public DateTime DateDebutAffectation { get; set; }
    public DateTime? DateFinAffectation { get; set; }
    public  required string Nature { get; set; }
    public bool IsActive { get; set; }
}
