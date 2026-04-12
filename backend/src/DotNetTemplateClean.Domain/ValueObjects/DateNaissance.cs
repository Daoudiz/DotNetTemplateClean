namespace DotNetTemplateClean.Domain;

public sealed record DateNaissance
{
    public const int MinimumAge = 18;
    public const int MaximumAge = 45;

    public DateOnly Value { get; }

    private DateNaissance(DateOnly value)
    {
        Value = value;
    }

    public static DateNaissance Create(DateOnly? date, DateOnly? referenceDate = null)
    {
        if (!date.HasValue)
        {
            throw new DomainException("La date de naissance est obligatoire.");
        }

        return Create(date.Value, referenceDate);
    }

    public static DateNaissance Create(DateOnly date, DateOnly? referenceDate = null)
    {
        var today = referenceDate ?? DateOnly.FromDateTime(DateTime.Today);

        ValidateAge(date, today);

        return new DateNaissance(date);
    }

    public static DateNaissance FromDateTime(DateTime? date, DateOnly? referenceDate = null)
    {
        if (!date.HasValue)
        {
            throw new DomainException("La date de naissance est obligatoire.");
        }

        return Create(DateOnly.FromDateTime(date.Value.Date), referenceDate);
    }

    public DateTime ToDateTime() => Value.ToDateTime(TimeOnly.MinValue);

    private static void ValidateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        var age = CalculateAge(birthDate, referenceDate);
        if (age < MinimumAge || age > MaximumAge)
        {
            throw new DomainException(
                $"L'age ({age} ans) doit etre compris entre {MinimumAge} et {MaximumAge} ans.");
        }
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate > referenceDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
