namespace DotNetTemplateClean.Application;

public static class PersonnelAffectationValidationExtensions
{
    private const string OverlappingAffectationsMessage =
        "Un personnel ne peut pas avoir deux affectations avec le meme couple entiteId/fonctionId sur des periodes qui se chevauchent.";
    public const string MissingActiveInitialEntiteAffectationMessage =
        "Le personnel doit avoir au moins une affectation active dans son entite initiale.";
    public const string AffectationStartBeforeRecruitmentDateMessage =
        "La date de debut d'affectation doit etre superieure ou egale a la date de recrutement.";
    public const string AffectationEndBeforeRecruitmentDateMessage =
        "La date de fin d'affectation doit etre superieure ou egale a la date de recrutement.";

    public static bool HasActiveAffectationForEntite<TAffectation>(
        IEnumerable<TAffectation>? affectations,
        int entiteId,
        Func<TAffectation, int> entiteIdSelector,
        Func<TAffectation, DateTime?> dateFinSelector)
    {
        if (entiteId <= 0 || affectations is null)
        {
            return false;
        }

        return affectations.Any(affectation =>
            entiteIdSelector(affectation) == entiteId
            && dateFinSelector(affectation) is null);
    }

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

    public static bool HasAffectationStartDatesOnOrAfterRecruitmentDate<TAffectation>(
        IEnumerable<TAffectation>? affectations,
        DateOnly? dateRecrutement,
        Func<TAffectation, DateTime> dateDebutSelector)
    {
        if (!dateRecrutement.HasValue || affectations is null)
        {
            return true;
        }

        var recruitmentDate = dateRecrutement.Value;

        return affectations.All(affectation =>
            DateOnly.FromDateTime(dateDebutSelector(affectation).Date) >= recruitmentDate);
    }

    public static bool HasAffectationEndDatesOnOrAfterRecruitmentDate<TAffectation>(
        IEnumerable<TAffectation>? affectations,
        DateOnly? dateRecrutement,
        Func<TAffectation, DateTime?> dateFinSelector)
    {
        if (!dateRecrutement.HasValue || affectations is null)
        {
            return true;
        }

        var recruitmentDate = dateRecrutement.Value;

        return affectations.All(affectation =>
        {
            var dateFin = dateFinSelector(affectation);
            return !dateFin.HasValue || DateOnly.FromDateTime(dateFin.Value.Date) >= recruitmentDate;
        });
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
