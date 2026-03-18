
namespace DotNetTemplateClean.Application;

    public interface IRoleService
    {
        /// <summary>
        /// Récupère la liste de tous les rôles.
        /// </summary>
        Task<IEnumerable<RoleResultDto>> GetAllRolesAsync();

    /// <summary>
    /// Récupère un rôle spécifique par son identifiant.
    /// </summary>
    Task<ServiceResult<RoleResultDto?>> GetRoleByIdAsync(string id);

        /// <summary>
        /// Crée un nouveau rôle.
        /// </summary>
        Task<ServiceResult<object?>> CreateRoleAsync(string roleName);

        /// <summary>
        /// Modifie le nom d'un rôle existant.
        /// </summary>
        Task<ServiceResult<object?>> UpdateRoleAsync(string id, string newName);

        /// <summary>
        /// Supprime physiquement un rôle de la base de données.
        /// </summary>
        Task<ServiceResult<object?>> DeleteRoleAsync(string id);
        Task<ServiceResult<RoleResultDto>> GetRoleByNameAsync(string roleName);
    }

