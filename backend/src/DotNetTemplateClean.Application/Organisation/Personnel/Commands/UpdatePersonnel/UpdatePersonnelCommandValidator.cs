namespace DotNetTemplateClean.Application;

public class UpdatePersonnelCommandValidator : AbstractValidator<UpdatePersonnelCommand>
{
    public UpdatePersonnelCommandValidator()
    {
        RuleFor(v => v.Affectations)
            .MustHaveUniqueEntiteFonctionPairs<UpdatePersonnelCommand, IList<UpdateAffectationDto>, UpdateAffectationDto>(
                affectation => affectation.EntiteId,
                affectation => affectation.FonctionId);
    }
}
