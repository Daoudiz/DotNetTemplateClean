using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;

namespace DotNetTemplateClean.Application.FunctionalTests.Organisation.Personnel.Authorization;

public class PersonnelCommandAuthorizationTests : TestBase
{
    [Test]
    public async Task ShouldThrowForbiddenForNonAdminUser()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = new CreatePersonnelCommand
        {
            Matricule = string.Empty,
            Nom = string.Empty,
            Prenom = string.Empty
        };

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowUnauthorizedWhenNoAuthenticatedUser()
    {
        await TestApp.ResetState();

        var command = new CreatePersonnelCommand
        {
            Matricule = string.Empty,
            Nom = string.Empty,
            Prenom = string.Empty
        };

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await TestApp.SendAsync(command));
    }
}
