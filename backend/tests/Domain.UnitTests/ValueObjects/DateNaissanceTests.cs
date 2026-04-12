using DotNetTemplateClean.Domain;

namespace CleanArchWebApi.Tests;

public class DateNaissanceTests
{
    [Test]
    public void CreateShouldSucceedWhenDateIsInPast()
    {
        var date = DateOnly.FromDateTime(DateTime.Today).AddYears(-25);

        var result = DateNaissance.Create(date);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(date));
    }

    [Test]
    public void CreateShouldThrowExceptionWhenDateIsNull()
    {
        DateOnly? date = null;

        var ex = Assert.Throws<DomainException>(() => DateNaissance.Create(date))!;

        Assert.That(ex.Message, Does.Contain("obligatoire"));
    }

    [Test]
    public void CreateShouldThrowExceptionWhenDateIsInFuture()
    {
        var date = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        var ex = Assert.Throws<DomainException>(() => DateNaissance.Create(date))!;

        Assert.That(ex.Message, Does.Contain("futur"));
    }
}
