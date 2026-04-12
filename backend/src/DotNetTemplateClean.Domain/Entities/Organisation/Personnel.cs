using System.Diagnostics.CodeAnalysis;

namespace DotNetTemplateClean.Domain;

public class Personnel : AuditableEntity<int>
{
    public required string Matricule { get; set; }
    public required string Nom { get; set; }
    public required string Prenom { get; set; }
    public DateOnly? DateRecrutement { get; set; }
    public DateNaissance? DateNaissance { get; set; }

    public string Email { get; init; } = string.Empty;
    public string? Statut { get; set; }
    public string? Grade { get; set; }
    // optional link to ApplicationUser
    public string? IdentityId { get; set; }

    // N-ary association: Affectation between Personnel, Entite and Fonction
    public ICollection<AffectationPersonnel> Affectations { get; } = [];

    public virtual int EntiteId { get; set; }

    public virtual Entite Entite { get; set; } = null!;

    [SetsRequiredMembers]
    private Personnel()
    {
        Matricule = string.Empty;
        Nom = string.Empty;
        Prenom = string.Empty;
    }

    public static Personnel Create(
        string matricule,
        string nom,
        string prenom,
        DateOnly dateRecrutement,
        DateNaissance dateNaissance,
        string email,
        int entiteId,
        string? statut,
        string? grade)
    {
        if (string.IsNullOrWhiteSpace(matricule))
        {
            throw new DomainException("Le matricule est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(nom))
        {
            throw new DomainException("Le nom est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(prenom))
        {
            throw new DomainException("Le prenom est obligatoire.");
        }

        ArgumentNullException.ThrowIfNull(dateNaissance);

        ValidateAgeAtRecruitment(dateNaissance, dateRecrutement );

        return new Personnel
        {
            Matricule = matricule,
            Nom = nom,
            Prenom = prenom,
            DateRecrutement = dateRecrutement,
            DateNaissance = dateNaissance,
            Email = email,
            Statut = statut,
            Grade = grade,
            EntiteId = entiteId
        };
    }

    private static void ValidateAgeAtRecruitment(DateNaissance dateNaissance, DateOnly dateRecrutement)
    {
        var age = dateRecrutement.Year - dateNaissance.Value.Year;

        if (dateNaissance.Value > dateRecrutement.AddYears(-age))
            age--;

        if (age < 18 || age > 45)
            throw new DomainException(
                "L'âge du personnel à la date de recrutement doit être compris entre 18 et 45 ans.");
    }
}
