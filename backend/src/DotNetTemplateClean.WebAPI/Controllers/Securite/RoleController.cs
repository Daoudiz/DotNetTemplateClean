using DotNetTemplateClean.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetTemplateClean.WebAPI;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController(IRoleService RoleService) : ApiBaseController
    {
        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleResultDto>>> GetAll()
        {

            // Le service s'occupe de tout : accès DB + transformation en DTO
            var roles = await RoleService.GetAllRolesAsync();

            // On retourne directement la liste propre
            return Ok(roles);
        }

        // GET: api/roles/{id}
        [HttpGet("{id}")]   
        public async Task<ActionResult<RoleResultDto>> GetById(string id)
        {
            var role = await RoleService.GetRoleByIdAsync(id);

            if (role == null)
            {
                // On reste sur un retour structuré pour l'erreur
                return NotFound(new { Message = $"Le rôle avec l'ID '{id}' est introuvable." });
            }

            // On retourne directement le DTO (le service doit s'occuper du mapping)
            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new { message = "Le nom du rôle est requis." });

            var result = await RoleService.CreateRoleAsync(roleName);

            return HandleResult(result);
        }

        // PUT: api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] string newName)
        {
            var result = await RoleService.UpdateRoleAsync(id, newName);

            return HandleResult(result);
        }

        // DELETE: api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await RoleService.DeleteRoleAsync(id);

            return HandleResult(result);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<RoleResultDto>> GetByName(string name)
        {
            var role = await RoleService.GetRoleByNameAsync(name);

            return HandleResult(role);
        }
    }

