namespace DotNetTemplateClean.Application;

public class CreatePersonnelCommandValidator : AbstractValidator<CreatePersonnelCommand>
{
    private readonly IApplicationDbContext _context;

    public CreatePersonnelCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Nom)
            .MaximumLength(100).WithMessage("Le nom ne doit pas depasser 100 caracteres.")
            .NotEmpty().WithMessage("Le nom est obligatoire.");

        RuleFor(v => v.Prenom)
            .MaximumLength(100).WithMessage("Le prenom ne doit pas depasser 100 caracteres.")
            .NotEmpty().WithMessage("Le prenom est obligatoire.");

        RuleFor(v => v.Matricule)
            .NotEmpty().WithMessage("Le matricule est obligatoire.")
            .MustAsync(BeUniqueMatricule).WithMessage("Ce matricule est deja utilise par un autre personnel.");

        RuleFor(v => v.DateNaissance)
            .Custom((dateNaissance, contextValidation) =>
            {
                try
                {
                    DateNaissance.Create(
                        dateNaissance.HasValue
                            ? DateOnly.FromDateTime(dateNaissance.Value.Date)
                            : null);
                }
                catch (DomainException ex)
                {
                    contextValidation.AddFailure(ex.Message);
                }
            });

        RuleFor(v => v.DateRecrutement)
            .NotEmpty()
            .Must((command, dateRecrutement) =>
            {
                if (!command.DateNaissance.HasValue || !dateRecrutement.HasValue)
                {
                    return true;
                }

                var birthDate = DateOnly.FromDateTime(command.DateNaissance.Value.Date);
                var recrutementDate = dateRecrutement.Value;

                return recrutementDate >= birthDate.AddYears(18);
            })
            .WithMessage("Le personnel doit avoir au moins 18 ans au moment du recrutement.");

        RuleForEach(v => v.Affectations).ChildRules(aff =>
        {
            aff.RuleFor(x => x.EntiteId).GreaterThan(0);
            aff.RuleFor(x => x.FonctionId).GreaterThan(0);
            aff.RuleFor(x => x.DateDebut).NotEmpty();
        });

        RuleFor(v => v.Affectations)
            .MustNotHaveOverlappingEntiteFonctionRanges<CreatePersonnelCommand, IList<CreateAffectationDto>, CreateAffectationDto>(
                affectation => affectation.EntiteId,
                affectation => affectation.FonctionId,
                affectation => affectation.DateDebut,
                _ => null);
    }

    public async Task<bool> BeUniqueMatricule(string matricule, CancellationToken cancellationToken)
    {
        return await _context.Personnels
            .AllAsync(l => l.Matricule != matricule, cancellationToken);
    }
}
