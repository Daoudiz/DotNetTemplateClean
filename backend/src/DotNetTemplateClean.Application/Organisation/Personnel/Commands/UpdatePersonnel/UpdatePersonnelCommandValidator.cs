namespace DotNetTemplateClean.Application;

public class UpdatePersonnelCommandValidator : AbstractValidator<UpdatePersonnelCommand>
{
    public UpdatePersonnelCommandValidator()
    {
        RuleFor(v => v.Affectations)
            .MustNotHaveOverlappingEntiteFonctionRanges<UpdatePersonnelCommand, IList<UpdateAffectationDto>, UpdateAffectationDto>(
                affectation => affectation.EntiteId,
                affectation => affectation.FonctionId,
                affectation => affectation.DateDebut,
                affectation => affectation.DateFinAffectation);
    }
}
