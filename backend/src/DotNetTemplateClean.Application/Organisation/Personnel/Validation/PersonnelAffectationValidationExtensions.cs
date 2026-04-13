namespace DotNetTemplateClean.Application;

public static class PersonnelAffectationValidationExtensions
{
    private const string OverlappingAffectationsMessage =
        "Un personnel ne peut pas avoir deux affectations avec le meme couple entiteId/fonctionId sur des periodes qui se chevauchent.";

    public static IRuleBuilderOptions<TCommand, TCollection> MustNotHaveOverlappingEntiteFonctionRanges<TCommand, TCollection, TAffectation>(
        this IRuleBuilder<TCommand, TCollection> ruleBuilder,
        Func<TAffectation, int> entiteIdSelector,
        Func<TAffectation, int> fonctionIdSelector,
        Func<TAffectation, DateTime> dateDebutSelector,
        Func<TAffectation, DateTime?> dateFinSelector)
        where TCollection : IEnumerable<TAffectation>
    {
        return ruleBuilder
            .Must(affectations => HaveNoOverlappingRanges(
                affectations,
                entiteIdSelector,
                fonctionIdSelector,
                dateDebutSelector,
                dateFinSelector))
            .WithMessage(OverlappingAffectationsMessage);
    }

    private static bool HaveNoOverlappingRanges<TAffectation>(
        IEnumerable<TAffectation>? affectations,
        Func<TAffectation, int> entiteIdSelector,
        Func<TAffectation, int> fonctionIdSelector,
        Func<TAffectation, DateTime> dateDebutSelector,
        Func<TAffectation, DateTime?> dateFinSelector)
    {
        if (affectations is null)
        {
            return true;
        }

        var groupedAffectations = new Dictionary<(int EntiteId, int FonctionId), List<(DateTime DateDebut, DateTime DateFin)>>();

        foreach (var affectation in affectations)
        {
            var entiteId = entiteIdSelector(affectation);
            var fonctionId = fonctionIdSelector(affectation);
            var dateDebut = dateDebutSelector(affectation);
            var dateFin = dateFinSelector(affectation) ?? DateTime.MaxValue;

            if (entiteId <= 0 || fonctionId <= 0)
            {
                continue;
            }

            if (dateFin < dateDebut)
            {
                return false;
            }

            var pairKey = (entiteId, fonctionId);
            if (!groupedAffectations.TryGetValue(pairKey, out var existingRanges))
            {
                existingRanges = [];
                groupedAffectations[pairKey] = existingRanges;
            }

            if (existingRanges.Any(existingRange => AreRangesOverlapping(existingRange.DateDebut, existingRange.DateFin, dateDebut, dateFin)))
            {
                return false;
            }

            existingRanges.Add((dateDebut, dateFin));
        }

        return true;
    }

    private static bool AreRangesOverlapping(DateTime firstStart, DateTime firstEnd, DateTime secondStart, DateTime secondEnd)
    {
        return firstStart <= secondEnd && secondStart <= firstEnd;
    }
}
