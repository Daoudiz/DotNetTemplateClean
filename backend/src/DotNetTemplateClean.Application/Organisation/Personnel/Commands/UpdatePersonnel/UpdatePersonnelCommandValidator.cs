namespace DotNetTemplateClean.Application;

public class UpdatePersonnelCommandValidator : AbstractValidator<UpdatePersonnelCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEntiteHierarchyService _entiteHierarchyService;

    public UpdatePersonnelCommandValidator(
        IApplicationDbContext context,
        IPersonnelMatriculeUniquenessService matriculeUniquenessService,
        IEntiteHierarchyService entiteHierarchyService)
    {
        _context = context;
        _entiteHierarchyService = entiteHierarchyService;

        RuleFor(v => v.Matricule)
            .NotEmpty().WithMessage("Le matricule est obligatoire.")
            .MustAsync((command, matricule, cancellationToken) =>
                matriculeUniquenessService.IsMatriculeUniqueAsync(matricule, command.Id, cancellationToken))
            .WithMessage("Ce matricule est deja utilise par un autre personnel.");

        RuleFor(v => v.Affectations)
            .MustNotHaveOverlappingEntiteFonctionRanges<UpdatePersonnelCommand, IList<UpdateAffectationDto>, UpdateAffectationDto>(
                affectation => affectation.EntiteId,
                affectation => affectation.FonctionId,
                affectation => affectation.DateDebut,
                affectation => affectation.DateFinAffectation);

        RuleFor(v => v)
            .Must(command => PersonnelAffectationValidationExtensions.HasAffectationStartDatesOnOrAfterRecruitmentDate(
                    command.Affectations,
                    command.DateRecrutement,
                    affectation => affectation.DateDebut))
            .WithMessage(PersonnelAffectationValidationExtensions.AffectationStartBeforeRecruitmentDateMessage);

        RuleFor(v => v)
            .Must(command => PersonnelAffectationValidationExtensions.HasAffectationEndDatesOnOrAfterRecruitmentDate(
                command.Affectations,
                command.DateRecrutement,
                affectation => affectation.DateFinAffectation))
            .WithMessage(PersonnelAffectationValidationExtensions.AffectationEndBeforeRecruitmentDateMessage);

        RuleFor(v => v)
            .MustAsync(HaveAffectationForRattachementHierarchy)
            .WithMessage(PersonnelAffectationValidationExtensions.MissingActiveRattachementHierarchyAffectationMessage);
    }

    private async Task<bool> HaveAffectationForRattachementHierarchy(
        UpdatePersonnelCommand command,
        CancellationToken cancellationToken)
    {
        var initialEntiteId = await _context.Personnels
            .Where(personnel => personnel.Id == command.Id)
            .Select(personnel => (int?)personnel.EntiteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!initialEntiteId.HasValue)
        {
            return true;
        }

        var allowedEntiteIds = await _entiteHierarchyService
            .GetFlattenedChildEntityIds(initialEntiteId.Value)
            .ConfigureAwait(false);

        return PersonnelAffectationValidationExtensions.HasActiveAffectationForAnyEntite(
            command.Affectations,
            allowedEntiteIds,
            affectation => affectation.EntiteId,
            affectation => affectation.DateFinAffectation);
    }
}
