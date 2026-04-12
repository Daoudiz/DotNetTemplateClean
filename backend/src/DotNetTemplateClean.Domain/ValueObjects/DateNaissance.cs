namespace DotNetTemplateClean.Domain;

public sealed record DateNaissance
{
    public DateOnly Value { get; }

    private DateNaissance(DateOnly value)
    {
        Value = value;
    }

    public static DateNaissance Create(DateOnly? date)
    {
        if (!date.HasValue)
            throw new DomainException("La date de naissance est obligatoire.");

        if (date.Value > DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("La date de naissance ne peut pas être dans le futur.");

        return new DateNaissance(date.Value);
    }

    internal static DateNaissance FromPersistence(DateOnly value)
        => new(value);
}
