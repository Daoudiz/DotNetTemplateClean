namespace DotNetTemplateClean.Application;

public static class PersonnelAffectationValidationExtensions
{
    private const string DuplicateAffectationsMessage =
        "Un personnel ne peut pas avoir deux affectations avec le meme couple entiteId/fonctionId.";

    public static IRuleBuilderOptions<TCommand, TCollection> MustHaveUniqueEntiteFonctionPairs<TCommand, TCollection, TAffectation>(
        this IRuleBuilder<TCommand, TCollection> ruleBuilder,
        Func<TAffectation, int> entiteIdSelector,
        Func<TAffectation, int> fonctionIdSelector)
        where TCollection : IEnumerable<TAffectation>
    {
        return ruleBuilder
            .Must(affectations => HaveUniqueEntiteFonctionPairs(affectations, entiteIdSelector, fonctionIdSelector))
            .WithMessage(DuplicateAffectationsMessage);
    }

    private static bool HaveUniqueEntiteFonctionPairs<TAffectation>(
        IEnumerable<TAffectation>? affectations,
        Func<TAffectation, int> entiteIdSelector,
        Func<TAffectation, int> fonctionIdSelector)
    {
        if (affectations is null)
        {
            return true;
        }

        var existingPairs = new HashSet<(int EntiteId, int FonctionId)>();

        foreach (var affectation in affectations)
        {
            var entiteId = entiteIdSelector(affectation);
            var fonctionId = fonctionIdSelector(affectation);

            if (entiteId <= 0 || fonctionId <= 0)
            {
                continue;
            }

            if (!existingPairs.Add((entiteId, fonctionId)))
            {
                return false;
            }
        }

        return true;
    }
}
