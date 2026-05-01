using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DotNetTemplateClean.Infrastructure;

public class UserService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    IJwtTokenService tokenService,
    IOptions<SearchSettings> searchOptions) : IUserService
{
    public async Task<PagedResult<UserSearchResultDto>> GetUsersBySearchModel(SearchViewModel searchModel)
    {
        if (searchModel == null)
        {
            return new PagedResult<UserSearchResultDto>([], 0);
        }

        var query = context.Users
            .AsNoTracking()
            .AsQueryable();

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(searchModel.Nom), u => searchModel.Nom != null && u.LastName.Contains(searchModel.Nom))
            .WhereIf(!string.IsNullOrWhiteSpace(searchModel.Prenom), u => searchModel.Prenom != null && u.FirstName.Contains(searchModel.Prenom));

        var projection = query.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            Nom = u.LastName,
            Prenom = u.FirstName,
            Email = u.Email ?? string.Empty,
            UserName = u.UserName ?? string.Empty,
            IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
            Roles = string.Join(", ", context.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Join(context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)),
            RoleId = context.UserRoles
                .Where(ur => ur.UserId == u.Id)
                .Select(ur => ur.RoleId)
                .FirstOrDefault() ?? string.Empty
        });

        var totalCount = await query.CountAsync();

        if (totalCount <= searchOptions.Value.ThresholdForFullLoad)
        {
            var all = await projection.ToListAsync();
            return new PagedResult<UserSearchResultDto>(all, totalCount, isFull: true);
        }

        var items = await projection
            .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
            .Take(searchModel.PageSize)
            .ToListAsync();

        return new PagedResult<UserSearchResultDto>(items, totalCount);
    }

    public async Task<ServiceResult<ApplicationUser>> GetUserById(string id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

        return user is null
            ? ServiceResult.Failure<ApplicationUser>(UserErrors.UserNotFound)
            : ServiceResult.Success(user);
    }

    public async Task<ServiceResult<string>> CreateUserWithRoleAsync(UserCreationDto userCreationDto)
    {
        ArgumentNullException.ThrowIfNull(userCreationDto);

        var user = new ApplicationUser
        {
            UserName = userCreationDto.UserName,
            Email = userCreationDto.Email,
            FirstName = userCreationDto.FirstName,
            LastName = userCreationDto.LastName,
            TwoFactorEnabled = userCreationDto.TwoFactorEnabled,
            MustChangePassword = userCreationDto.MustChangePasswordOnFirstLogin
        };

        var createResult = await userManager.CreateAsync(user, userCreationDto.Password);

        if (!createResult.Succeeded)
        {
            return ServiceResult.Failure<string>(
                createResult.Errors.FirstOrDefault()?.Description ?? "Erreur lors de la creation de l'utilisateur",
                400);
        }

        var role = await roleManager.FindByIdAsync(userCreationDto.UserRole);
        if (role is null)
        {
            await userManager.DeleteAsync(user);

            return ServiceResult.Failure<string>(
                UserErrors.RoleNotFound,
                404);
        }

        var roleResult = await userManager.AddToRoleAsync(user, role.Name!);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return ServiceResult.Failure<string>(
                UserErrors.RoleAssignmentFailed,
                500);
        }

        return ServiceResult.Success(user.Id, 201);
    }

    public async Task<ServiceResult<object?>> DeleteUserAsync(string id, string currentUserId)
    {
        if (id == currentUserId)
        {
            return ServiceResult.Failure(UserErrors.CannotDisableSelf, 400);
        }

        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            return ServiceResult.Failure(UserErrors.UserNotFound, 404);
        }

        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd == DateTimeOffset.MaxValue;
        DateTimeOffset? newLockoutDate = isLocked ? null : DateTimeOffset.MaxValue;

        var lockResult = await userManager.SetLockoutEndDateAsync(user, newLockoutDate);

        if (!lockResult.Succeeded)
        {
            return ServiceResult.Failure(UserErrors.ErrorDesictivated, 400);
        }

        await userManager.UpdateSecurityStampAsync(user);

        return ServiceResult.Success(200);
    }

    public async Task<ServiceResult<string>> UpdateUserAsync(UserUpdateDto dto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var user = await userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            return ServiceResult.Failure<string>(UserErrors.UserNotFound, 404);
        }

        var newRole = await roleManager.FindByIdAsync(dto.UserRole);
        if (newRole == null)
        {
            return ServiceResult.Failure<string>(UserErrors.RoleNotFound, 404);
        }

        using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            user.LastName = dto.LastName;
            user.FirstName = dto.FirstName;
            user.UserName = dto.UserName;
            user.Email = dto.Email;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync(ct);
                var errors = string.Join(" | ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult.Failure<string>(errors, 400);
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(newRole.Name!))
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                var roleResult = await userManager.AddToRoleAsync(user, newRole.Name!);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync(ct);
                    return ServiceResult.Failure<string>("Echec lors de l'attribution du role.", 400);
                }
            }

            await transaction.CommitAsync(ct);

            return ServiceResult.Success<string>("Utilisateur mis a jour avec succes.");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(ct);
            return ServiceResult.Failure<string>($"Erreur technique : {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<object?>> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure(UserErrors.UserNotFound, 404);
        }

        var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            return ServiceResult.Failure(errors, 400);
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            var clearFlagResult = await userManager.UpdateAsync(user);
            if (!clearFlagResult.Succeeded)
            {
                var errors = string.Join(" | ", clearFlagResult.Errors.Select(e => e.Description));
                return ServiceResult.Failure(errors, 400);
            }
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<bool>> UnlockUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.Failure<bool>(UserErrors.UserNotFound, 404);
        }

        if (!await userManager.IsLockedOutAsync(user))
        {
            return ServiceResult.Failure<bool>("Ce compte n'est pas verrouille.", 400);
        }

        var result = await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);

        if (result.Succeeded)
        {
            return ServiceResult.Success(true, 200);
        }

        return ServiceResult.Failure<bool>("Erreur lors du deverrouillage.", 500);
    }

    public async Task<ServiceResult<ProfilViewModel>> GetUserProfileAsync(string userId, string userRole)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return ServiceResult.Failure<ProfilViewModel>(UserErrors.UserNotFound, 404);
        }

        var profileModel = new ProfilViewModel
        {
            Id = user.Id,
            LastName = user.LastName,
            FirstName = user.FirstName,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Password = null,
            UserRole = userRole
        };

        return ServiceResult.Success(profileModel);
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);
        }

        var check = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (check.IsLockedOut)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.AccountLocked, 423);
        }

        if (check.RequiresTwoFactor)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.TwoFARequired, 403);
        }

        if (!check.Succeeded)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);
        }

        if (user.MustChangePassword)
        {
            return ServiceResult.Success(new LoginResponseDto
            {
                PasswordChangeRequired = true,
                Username = user.UserName ?? string.Empty
            });
        }

        var tokenData = await tokenService.GenerateTokenAsync(user);

        var response = new LoginResponseDto
        {
            PasswordChangeRequired = false,
            Token = tokenData.Token,
            Expires = tokenData.ExpiresAt,
            Username = user.UserName ?? string.Empty,
            Roles = tokenData.Roles
        };

        return ServiceResult.Success(response);
    }

    public async Task<ServiceResult<LoginResponseDto>> FirstLoginChangePasswordAndLoginAsync(FirstLoginChangePasswordViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await userManager.FindByNameAsync(model.UserName);
        if (user is null)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);
        }

        if (!user.MustChangePassword)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.PasswordChangeRequired, 400);
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return ServiceResult.Failure<LoginResponseDto>("La confirmation du mot de passe ne correspond pas.", 400);
        }

        var check = await signInManager.CheckPasswordSignInAsync(user, model.OldPassword, false);
        if (!check.Succeeded)
        {
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);
        }

        var changeResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!changeResult.Succeeded)
        {
            var errors = string.Join(" | ", changeResult.Errors.Select(e => e.Description));
            return ServiceResult.Failure<LoginResponseDto>(errors, 400);
        }

        user.MustChangePassword = false;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(" | ", updateResult.Errors.Select(e => e.Description));
            return ServiceResult.Failure<LoginResponseDto>(errors, 400);
        }

        var tokenData = await tokenService.GenerateTokenAsync(user);
        var response = new LoginResponseDto
        {
            PasswordChangeRequired = false,
            Token = tokenData.Token,
            Expires = tokenData.ExpiresAt,
            Username = user.UserName ?? string.Empty,
            Roles = tokenData.Roles
        };

        return ServiceResult.Success(response);
    }

    public async Task<ServiceResult<bool>> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return ServiceResult.Success(true, 204);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await userClaimsPrincipalFactory.CreateAsync(user);
        var result = await authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }
}
