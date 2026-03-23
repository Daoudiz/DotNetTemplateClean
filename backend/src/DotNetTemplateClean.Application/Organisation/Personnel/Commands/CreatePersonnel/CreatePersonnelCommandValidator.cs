
namespace DotNetTemplateClean.Application;

public class CreatePersonnelCommandValidator : AbstractValidator<CreatePersonnelCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePersonnelCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Nom)
            .MaximumLength(100).WithMessage("Le nom ne doit pas dépasser 100 caractères.")
            .NotEmpty().WithMessage("Le nom est obligatoire.");

        RuleFor(v => v.Prenom)
            .MaximumLength(100).WithMessage("Le prénom ne doit pas dépasser 100 caractères.")
            .NotEmpty().WithMessage("Le prénom est obligatoire.");

        RuleFor(v => v.Matricule)
            .NotEmpty().WithMessage("Le matricule est obligatoire.")
            .MustAsync(BeUniqueMatricule).WithMessage("Ce matricule est déjà utilisé par un autre personnel.");

        RuleFor(v => v.DateRecrutement)
            .NotEmpty()
            .Must((command, dateRecrutement) =>
            {
                // Si l'une des deux dates est nulle, on laisse passer (les autres rules s'en occupent)
                if (!command.DateNaissance.HasValue || !dateRecrutement.HasValue)
                    return true;

                // La règle : Recrutement >= Naissance + 18 ans
                return dateRecrutement >= command.DateNaissance.Value.AddYears(18);
            })
            .WithMessage("Le personnel doit avoir au moins 18 ans au moment du recrutement.");

        // Validation de la liste d'affectations
        RuleForEach(v => v.Affectations).ChildRules(aff => {
            aff.RuleFor(x => x.EntiteId).GreaterThan(0);
            aff.RuleFor(x => x.FonctionId).GreaterThan(0);
            aff.RuleFor(x => x.DateDebut).NotEmpty();
        });

    }

    // Validation asynchrone pour vérifier l'unicité en base de données
    public async Task<bool> BeUniqueMatricule(string matricule, CancellationToken cancellationToken)
    {
        return await _context.Personnels
            .AllAsync(l => l.Matricule != matricule, cancellationToken);
    }
}
