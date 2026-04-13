namespace DotNetTemplateClean.Application;

public class UpdatePersonnelCommandValidator : AbstractValidator<UpdatePersonnelCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePersonnelCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(v => v.Affectations)
            .MustNotHaveOverlappingEntiteFonctionRanges<UpdatePersonnelCommand, IList<UpdateAffectationDto>, UpdateAffectationDto>(
                affectation => affectation.EntiteId,
                affectation => affectation.FonctionId,
                affectation => affectation.DateDebut,
                affectation => affectation.DateFinAffectation);

        RuleFor(v => v)
            .MustAsync(HaveAffectationForInitialEntite)
            .WithMessage(PersonnelAffectationValidationExtensions.MissingActiveInitialEntiteAffectationMessage);
    }

    private async Task<bool> HaveAffectationForInitialEntite(UpdatePersonnelCommand command, CancellationToken cancellationToken)
    {
        var initialEntiteId = await _context.Personnels
            .Where(personnel => personnel.Id == command.Id)
            .Select(personnel => (int?)personnel.EntiteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!initialEntiteId.HasValue)
        {
            return true;
        }

        return PersonnelAffectationValidationExtensions.HasActiveAffectationForEntite(
            command.Affectations,
            initialEntiteId.Value,
            affectation => affectation.EntiteId,
            affectation => affectation.DateFinAffectation);
    }
}
