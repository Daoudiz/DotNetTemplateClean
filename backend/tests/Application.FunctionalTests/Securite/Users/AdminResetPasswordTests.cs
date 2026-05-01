using DotNetTemplateClean.Application.FunctionalTests.Infrastructure;
using DotNetTemplateClean.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetTemplateClean.Application.FunctionalTests.Securite.Users;

public class AdminResetPasswordTests : TestBase
{
    [Test]
    public async Task AdminResetPassword_ShouldResetPassword_AndRequirePasswordChange()
    {
        var userId = await CreateUserAsync("reset-ok@local", "Initial1234!");
        var newPassword = "TempReset#2026A";

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.AdminResetPasswordAsync(userId, new AdminResetPasswordViewModel
        {
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.UserId, Is.EqualTo(userId));
            Assert.That(result.Value.TemporaryPassword, Is.EqualTo(newPassword));
            Assert.That(result.Value.MustChangePassword, Is.True);
        });

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var updated = await userManager.FindByIdAsync(userId);
        Assert.That(updated, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updated!.MustChangePassword, Is.True);
            Assert.That(updated.PasswordHash, Is.Not.Null.And.Not.Empty);
            Assert.That(updated.PasswordHash, Does.Not.Contain(newPassword));
        });
    }

    [Test]
    public async Task AdminResetPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.AdminResetPasswordAsync(Guid.NewGuid().ToString(), new AdminResetPasswordViewModel
        {
            NewPassword = "TempReset#2026A",
            ConfirmPassword = "TempReset#2026A"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task AdminResetPassword_ShouldReturnValidationError_WhenPasswordPolicyFails()
    {
        var userId = await CreateUserAsync("reset-policy@local", "Initial1234!");

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var result = await userService.AdminResetPasswordAsync(userId, new AdminResetPasswordViewModel
        {
            NewPassword = "123",
            ConfirmPassword = "123"
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
        });
    }

    private static async Task<string> CreateUserAsync(string email, string password)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole is null)
        {
            adminRole = new IdentityRole("Admin");
            var createRole = await roleManager.CreateAsync(adminRole);
            if (!createRole.Succeeded)
            {
                throw new InvalidOperationException("Unable to create Admin role for tests.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Reset",
            LastName = "Target",
            MustChangePassword = false
        };

        var createUser = await userManager.CreateAsync(user, password);
        if (!createUser.Succeeded)
        {
            var errors = string.Join(" | ", createUser.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Unable to create test user. {errors}");
        }

        await userManager.AddToRoleAsync(user, "Admin");

        var saved = await context.Users.AsNoTracking().SingleAsync(x => x.Email == email);
        return saved.Id;
    }
}
