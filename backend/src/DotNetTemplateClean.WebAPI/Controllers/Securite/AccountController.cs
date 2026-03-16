
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DotNetTemplateClean.Application;
using System.Security.Claims;


namespace DotNetTemplateClean.WebAPI;

[ApiController]
[Authorize]
[Route("api/[controller]")]
internal class AccountController(IUserService UserService) : ApiBaseController
{
    // Simple health check
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok(new { message = "Account API is available." });

    // POST api/account/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        
        if (model is null) return BadRequest("Invalid payload.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        var result = await UserService.LoginAsync(model);
       
        return HandleResult(result);
    }

    // POST api/account/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var result = await UserService.LogoutAsync();
        return HandleResult(result);
    }
    // GET: api/acount/profile
    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ProfilViewModel?>> GetProfile()
    {

        //Extraction sécurisée du UID et du ROLE depuis le Token
        // On utilise "uid" car c'est ce que tu as défini dans ta génération de token
        var userId = User.FindFirst("uid")?.Value;

        // ClaimTypes.Role récupère automatiquement le claim "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        var currentRole = User.FindFirstValue(ClaimTypes.Role) ?? "N/A";

        if (string.IsNullOrEmpty(userId))
        {
            return Problem(detail: "Utilisateur non identifié dans le jeton.", statusCode: 401);
        }

        var result = await UserService.GetUserProfileAsync(userId, currentRole);

        //Traitement de l'erreur via la méthode commune
        return HandleResult(result);         

    }
   
}
