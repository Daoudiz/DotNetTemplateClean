
using DotNetTemplateClean.Domain;

namespace CleanArchWebApi.Tests; 

public class DateNaissanceTests
{
    // 🎯 Helper pour générer une date à partir d'un âge
    private static DateOnly GetDateFromAge(int age)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return today.AddYears(-age);
    }

    // ✅ Cas valide
    [Test]
    public void CreateShouldSucceedWhenAgeIsBetween18And45()
    {
        var date = GetDateFromAge(25);

        var result = DateNaissance.Create(date);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(date));
    }

    // ❌ Age < 18
    [Test]
    public void CreateShouldThrowExceptionWhenAgeIsLessThan18()
    {
        var date = GetDateFromAge(17);

        var ex = Assert.Throws<DomainException>(() =>
            DateNaissance.Create(date))!;

        Assert.That(ex.Message, Does.Contain("18"));
    }

    // ❌ Age > 45
    [Test]
    public void CreateShouldThrowExceptionWhenAgeIsGreaterThan45()
    {
        var date = GetDateFromAge(46);

        var ex = Assert.Throws<DomainException>(() =>
            DateNaissance.Create(date))!;

        Assert.That(ex.Message, Does.Contain("45"));
    }

    // ✅ Limite basse (18 ans)
    [Test]
    public void CreateShouldSucceedWhenAgeIsExactly18()
    {
        var date = GetDateFromAge(18);

        var result = DateNaissance.Create(date);

        Assert.That(result, Is.Not.Null);
    }

    // ✅ Limite haute (45 ans)
    [Test]
    public void CreateShouldSucceedWhenAgeIsExactly45()
    {
        var date = GetDateFromAge(45);

        var result = DateNaissance.Create(date);

        Assert.That(result, Is.Not.Null);
    }

    // ❌ Null
    [Test]
    public void CreateShouldThrowExceptionWhenDateIsNull()
    {
        DateOnly? date = null;

        var ex = Assert.Throws<DomainException>(() =>
            DateNaissance.Create(date))!;

        Assert.That(ex.Message, Does.Contain("obligatoire"));
    }
}
