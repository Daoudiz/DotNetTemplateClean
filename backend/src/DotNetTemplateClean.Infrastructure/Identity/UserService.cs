
using System.Globalization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DotNetTemplateClean.Infrastructure;

public class UserService(ApplicationDbContext  context,
                        IEntiteService entiteService,
                        UserManager<ApplicationUser> UserManager,
                        SignInManager<ApplicationUser> SignInManager,
                        RoleManager<IdentityRole> RoleManager,
                        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
                        IAuthorizationService authorizationService,
                         IJwtTokenService TokenService,
                        IOptions<SearchSettings> SearchOptions) :  IUserService
{
   
    public async Task<PagedResult<UserSearchResultDto>> GetUsersBySearchModel(SearchViewModel searchModel)
    {
        if (searchModel == null)
        {
            return new PagedResult<UserSearchResultDto>([], 0);
        }

        var query = context.Users.Include(u => u.Entite)
                                  .ThenInclude(e => e!.Rattachement)
                                  .ThenInclude(r => r!.Rattachement)
                                  .AsNoTracking()
                                  .AsQueryable();

        query = query.WhereIf(searchModel.Matricule != null, u => u.Matricule == searchModel.Matricule)
                     .WhereIf(!string.IsNullOrWhiteSpace(searchModel.Nom), u => searchModel.Nom != null && u.LastName.Contains(searchModel.Nom))
                     .WhereIf(!string.IsNullOrWhiteSpace(searchModel.Prenom), u => searchModel.Prenom != null && u.FirstName.Contains(searchModel.Prenom))
                     .WhereIf(searchModel.DateRecrutementDebut != null, u => u.DateRecrutement >= searchModel.DateRecrutementDebut)
                     .WhereIf(searchModel.DateRecrutementFin != null, u => u.DateRecrutement <= searchModel.DateRecrutementFin);

        int? targetId = searchModel.ServiceId ?? searchModel.DivisionId ?? searchModel.DirectionId;

        if (targetId.HasValue)
        {
            var allIdsInBranch = await entiteService.GetFlattenedChildEntityIds(targetId.Value);
            query = query.Where(u => allIdsInBranch.Contains(u.EntiteId));
        }

        var projection = query.Select(u => new UserSearchResultDto
        {
            Id = u.Id,
            Matricule = u.Matricule,
            Nom = u.LastName,
            Prenom = u.FirstName,
            DateRecrutement = u.DateRecrutement,

            // Changement : On compare le Libelle du TypeEntite
            ServiceId = u.Entite.TypeEntite.Libelle == "Service" ? u.EntiteId : (int?)null,

            // Logique DivisionId
            DivisionId = u.Entite.TypeEntite.Libelle == "Service" ? u.Entite.RattachementEntiteId :
             u.Entite.TypeEntite.Libelle == "Division" ? u.EntiteId : (int?)null,

            // Logique DirectionId (Simplifiée pour la traduction SQL)
            DirectionId = (int)(u.Entite.TypeEntite.Libelle == "Direction" ? u.EntiteId :
              (u.Entite.Rattachement != null && u.Entite.Rattachement.TypeEntite.Libelle == "Direction")
                  ? u.Entite.RattachementEntiteId :
                  (u.Entite.Rattachement != null && u.Entite.Rattachement.Rattachement != null
                      ? u.Entite.Rattachement.RattachementEntiteId : 0))!,

            Email = u.Email ?? string.Empty,
            UserName = u.UserName ?? string.Empty,

            // Verrouillage
            IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,

            // Rôles (Attention : string.Join n'est pas toujours traduisible en SQL selon votre provider EF)
            Roles =   string.Join(", ", context.UserRoles
                            .Where(ur => ur.UserId == u.Id)
                            .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)),
                                    RoleId = context.UserRoles
                            .Where(ur => ur.UserId == u.Id)
                            .Select(ur => ur.RoleId)
                            .FirstOrDefault() ?? string.Empty
        });              

        int totalCount = await query.CountAsync();

        //Gestion du Full Load vs Pagination
        if (totalCount <= SearchOptions.Value.ThresholdForFullLoad)
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
         var user = await  context.Users.FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? ServiceResult.Failure<ApplicationUser>(UserErrors.UserNotFound) 
                            : ServiceResult.Success(user);        
            
    }

    public async Task<ServiceResult<string>> CreateUserWithRoleAsync(UserCreationDto userCreationDto)
    {
        ArgumentNullException.ThrowIfNull(userCreationDto);

        // Vérification métier : unicité du matricule
        var alreadyExists = await MatriculeExistsAsync(userCreationDto.Matricule);
        if (alreadyExists)
        {
            return ServiceResult.Failure<string>(
                string.Format(CultureInfo.InvariantCulture, UserErrors.DuplicateMatricule, userCreationDto.Matricule),
                409);
        }

        // Création de l'utilisateur
        var user = new ApplicationUser
        {
            UserName = userCreationDto.UserName,
            Email = userCreationDto.Email,
            FirstName = userCreationDto.FirstName,
            LastName = userCreationDto.LastName,
            Matricule = userCreationDto.Matricule,
            DateRecrutement = userCreationDto.DateRecrutement,
            EntiteId = userCreationDto.Service
                       ?? userCreationDto.Division
                       ?? userCreationDto.Direction
        };

        var createResult = await UserManager.CreateAsync(user, userCreationDto.Password);

        if (!createResult.Succeeded)
        {
            return ServiceResult.Failure<string>(
                createResult.Errors.FirstOrDefault()?.Description ?? "Erreur lors de la création de l'utilisateur",
                400);
        }

        // Vérification du rôle
        var role = await RoleManager.FindByIdAsync(userCreationDto.UserRole);
        if (role is null)
        {
            // Atomicité : suppression de l'utilisateur si le rôle n'existe pas
            await UserManager.DeleteAsync(user);

            return ServiceResult.Failure<string>(
                UserErrors.RoleNotFound,
                404);
        }

        // Attribution du rôle
        var roleResult = await UserManager.AddToRoleAsync(user, role.Name!);
        if (!roleResult.Succeeded)
        {
            // Atomicité : suppression de l'utilisateur si l'ajout du rôle échoue
            await UserManager.DeleteAsync(user);

            return ServiceResult.Failure<string>(
                UserErrors.RoleAssignmentFailed,
                500);
        }

        return ServiceResult.Success(user.Id, 201);
    }


    //Désative ou active le compte d'un utilisateur
    public async Task<ServiceResult<object?>> DeleteUserAsync(string id, string currentUserId)
    {
        // Empêcher l'auto-désactivation
        if (id == currentUserId)
        {
            return ServiceResult.Failure(UserErrors.CannotDisableSelf, 400);
        }

        var user = await UserManager.FindByIdAsync(id);

        if (user == null)
        {
            return ServiceResult.Failure(UserErrors.UserNotFound, 404);
        }

        // Déterminer si l'utilisateur est actuellement verrouillé "définitivement"
        // On vérifie si LockoutEnd est égal à MaxValue
        bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd == DateTimeOffset.MaxValue;

        DateTimeOffset? newLockoutDate;

        if (isLocked)
        {
            // --- CAS : RÉACTIVATION ---
            // On remet la date à null pour déverrouiller
            newLockoutDate = null;
        }
        else
        {
            // --- CAS : DÉSACTIVATION ---
            // On verrouille jusqu'en 9999
            newLockoutDate = DateTimeOffset.MaxValue;
        }

        //Appliquer la nouvelle date
        var lockResult = await UserManager.SetLockoutEndDateAsync(user, newLockoutDate);

        if (!lockResult.Succeeded)
        {
            return ServiceResult.Failure(UserErrors.ErrorDesictivated, 400);
        }

        //Sécurité : Invalider la session actuelle
        // Qu'on active ou désactive, il est préférable de forcer une reconnexion
        // pour mettre à jour les Claims et le jeton de sécurité.
        await UserManager.UpdateSecurityStampAsync(user); 

        return ServiceResult.Success(200);
    }

    public async Task<ServiceResult<string>> UpdateUserAsync(UserUpdateDto dto, CancellationToken ct)
    {
        // Validation de l'entrée (Anti-Null)
        ArgumentNullException.ThrowIfNull(dto);

        // Vérification d'existence et de duplicata (Lecture seule, hors transaction)
        var user = await UserManager.FindByIdAsync(dto.UserId);
        if (user == null) return ServiceResult.Failure<string>(UserErrors.UserNotFound, 404);

        var alreadyExists = await MatriculeExistsAsync(dto.Matricule, dto.UserId);
        if (alreadyExists)
        {
            return ServiceResult.Failure<string>(
                string.Format(CultureInfo.InvariantCulture, UserErrors.DuplicateMatricule, dto.Matricule),
                409);
        }

        var newRole = await RoleManager.FindByIdAsync(dto.UserRole);
        if (newRole == null) return ServiceResult.Failure<string>(UserErrors.RoleNotFound, 404);

        // Début de l'opération atomique
        using var transaction = await context.Database.BeginTransactionAsync(ct);

        try
        {
            // Mise à jour des propriétés de l'objet (en mémoire)
            user.Matricule = dto.Matricule;
            user.LastName = dto.LastName;
            user.FirstName = dto.FirstName;
            user.UserName = dto.UserName;
            user.DateRecrutement = dto.DateRecrutement;
            user.Email = dto.Email;
            user.EntiteId = dto.Service ?? dto.Division ?? dto.Direction;

            // Persistance de l'utilisateur
            var updateResult = await UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                await transaction.RollbackAsync(ct);
                var errors = string.Join(" | ", updateResult.Errors.Select(e => e.Description));
                return ServiceResult.Failure<string>(errors, 400);
            }

            // Mise à jour des rôles
            var currentRoles = await UserManager.GetRolesAsync(user);
            if (!currentRoles.Contains(newRole.Name!))
            {
                await UserManager.RemoveFromRolesAsync(user, currentRoles);
                var roleResult = await UserManager.AddToRoleAsync(user, newRole.Name!);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync(ct);
                    return ServiceResult.Failure<string>("Échec lors de l'attribution du rôle.", 400);
                }
            }

            // Validation finale en base de données
            await transaction.CommitAsync(ct);

            return ServiceResult.Success<string>("Utilisateur mis à jour avec succès.");
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException)
        {
            // En cas d'erreur technique imprévue (ex: perte SQL), on annule tout
            await transaction.RollbackAsync(ct);
            return ServiceResult.Failure<string>($"Erreur technique : {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<object?>> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return ServiceResult.Failure(UserErrors.UserNotFound, 404);

        var result = await UserManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
            return ServiceResult.Failure(errors, 400);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<bool>> UnlockUserAsync(string userId)
    {
        //Recherche de l'utilisateur
        var user = await UserManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResult.Failure<bool>(UserErrors.UserNotFound, 404);

        //Vérification si l'utilisateur est réellement verrouillé
        if (!await UserManager.IsLockedOutAsync(user))
        {
            return ServiceResult.Failure<bool>("Ce compte n'est pas verrouillé.", 400);
        }

        //Déverrouillage : on fixe la date de fin à "maintenant" ou null
        // On réinitialise aussi le compteur d'échecs (AccessFailedCount)
        var result = await UserManager.SetLockoutEndDateAsync(user, null);
        await UserManager.ResetAccessFailedCountAsync(user);

        if (result.Succeeded)
        {
            return ServiceResult.Success<bool>(true,200);
        }

        return ServiceResult.Failure<bool>("Erreur lors du déverrouillage.", 500);
    }


    public async Task<ServiceResult<ProfilViewModel>> GetUserProfileAsync(string userId, string userRole)
    {

        var oUser = await UserManager.FindByIdAsync(userId);

        if (oUser == null)
        {
            return ServiceResult.Failure<ProfilViewModel>(
                UserErrors.UserNotFound, // Utilisation de tes constantes
                404);
        }


        var profileModel = new ProfilViewModel
        {
            Id = oUser.Id,
            Matricule = oUser.Matricule,
            LastName = oUser.LastName,
            FirstName = oUser.FirstName ?? string.Empty,
            UserName = oUser.UserName ?? string.Empty,
            Email = oUser.Email ?? string.Empty,
            Password = null,
            UserRole = userRole,
            DateRecrutement = oUser.DateRecrutement,
            Entite = string.Empty
        };


        if (oUser.EntiteId != 0)
        {
            var result = await entiteService.GetEntiteById(oUser.EntiteId);
            profileModel.Entite = result.Value?.Libelle ?? "Entité inconnue";
        }

        return ServiceResult.Success<ProfilViewModel>(profileModel);
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var user = await UserManager.FindByNameAsync(model.UserName);
        if (user == null)
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);

        var check = await SignInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (check.IsLockedOut)
            return ServiceResult.Failure<LoginResponseDto>(Auth.AccountLocked, 423);

        if (check.RequiresTwoFactor)
            // Ici, on pourrait soit renvoyer un code spécial, soit un succès partiel
            return ServiceResult.Failure<LoginResponseDto>(Auth.TwoFARequired, 403);

        if (!check.Succeeded)
            return ServiceResult.Failure<LoginResponseDto>(Auth.InvalidCredentials, 401);

        // Tout est OK, on génère le token
        var tokenData = await TokenService.GenerateTokenAsync(user);

        var response = new LoginResponseDto
        {
            Token = tokenData.Token,
            Expires = tokenData.ExpiresAt,
            Username = user.UserName ?? string.Empty,
            Roles = tokenData.Roles
        };

        return ServiceResult.Success<LoginResponseDto>(response);
    }

    public async Task<ServiceResult<bool>> LogoutAsync()
    {
        // On appelle Identity pour nettoyer les éventuels cookies 
        // (même si on utilise JWT, c'est une sécurité si ton app mélange les deux)
        await SignInManager.SignOutAsync();

        // On retourne un succès. On utilise 'bool' ou 'object?' comme type générique.
        return ServiceResult.Success<bool>(true, 204);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await UserManager.FindByIdAsync(userId);

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
        var user = await UserManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    #region Helpers

    public async Task<bool> MatriculeExistsAsync(int matricule, string? currentUserId = null)
    {
        var query = context.Users.AsNoTracking().Where(u => u.Matricule == matricule);

        // Si on fournit un ID, on l'exclut de la recherche
        if (!string.IsNullOrEmpty(currentUserId))
        {
            query = query.Where(u => u.Id != currentUserId);
        }

        return await query.AnyAsync();
    }
    #endregion
}
