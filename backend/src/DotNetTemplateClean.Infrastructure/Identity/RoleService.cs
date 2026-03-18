
namespace DotNetTemplateClean.Infrastructure;

public class RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager) : IRoleService
{
    // READ ALL
    public async Task<IEnumerable<RoleResultDto>> GetAllRolesAsync()
    {
        if (roleManager.Roles == null)
            return [];

        return await roleManager.Roles
        .Select(r => new RoleResultDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty
        })
        .ToListAsync(); // Récupère tous les rôles d'un coup

    }

    // READ ONE
    public async Task<ServiceResult<RoleResultDto?>> GetRoleByIdAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null)
            return ServiceResult.Failure<RoleResultDto?>(Role.RoleNotFound, 404);

        var RoleResultDto =  new RoleResultDto
        {
            Id = role.Id ?? string.Empty,
            Name = role.Name ?? string.Empty
        };

        return ServiceResult.Success<RoleResultDto?>(RoleResultDto);

    }

    // CREATE
    public async Task<ServiceResult<object?>> CreateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return ServiceResult.Failure(Role.RoleNameMissed, 400);

        if (await roleManager.RoleExistsAsync(roleName))
            return ServiceResult.Failure(Role.RoleAlreadyExists, 409);

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));

        return result.Succeeded
            ? ServiceResult.Success(201)
            : ServiceResult.Failure(FormatIdentityErrors(result), 400);
    }

    // UPDATE
    public async Task<ServiceResult<object?>> UpdateRoleAsync(string id, string newName)
    {

        if(newName == null || string.IsNullOrWhiteSpace(newName))
            return ServiceResult.Failure(Role.RoleNameMissed, 400);

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
            return ServiceResult.Failure(Role.RoleNotFound, 404);

        role.Name = newName;
        // On met aussi à jour le NormalizedName pour garantir la cohérence d'Identity
        role.NormalizedName = newName.ToUpper(System.Globalization.CultureInfo.CurrentCulture);

        var result = await roleManager.UpdateAsync(role);

        return result.Succeeded
            ? ServiceResult.Success(200)
            : ServiceResult.Failure(FormatIdentityErrors(result), 400);
    }

    // DELETE (HARD DELETE)
    public async Task<ServiceResult<object?>> DeleteRoleAsync(string id)
    {
       
        var role = await roleManager.FindByIdAsync(id);

        if (role == null)
            return ServiceResult.Failure(Role.RoleNotFound, 404);

        // Vérifier si des utilisateurs possèdent ce rôle            
        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);

        if (usersInRole.Any())
        {
            return ServiceResult.Failure(
                $"Impossible de supprimer le rôle '{role.Name}' car il est assigné à {usersInRole.Count} utilisateur(s).",
                400);
        }

        //Suppression physique si aucun lien n'existe
        var result = await roleManager.DeleteAsync(role);

        return result.Succeeded
            ? ServiceResult.Success(200)
            : ServiceResult.Failure(FormatIdentityErrors(result), 400);
    }

    public async Task<ServiceResult<RoleResultDto>> GetRoleByNameAsync(string roleName)
    {
        var role = await roleManager.FindByNameAsync(roleName);

        if (role == null)
            return ServiceResult.Failure<RoleResultDto>(Role.RoleNotFound, 404);
       
        var roleDto = new RoleResultDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        };

        return ServiceResult.Success<RoleResultDto>(roleDto);
    }

    private static  string FormatIdentityErrors(IdentityResult result)
        => string.Join(" | ", result.Errors.Select(e => e.Description));
}
