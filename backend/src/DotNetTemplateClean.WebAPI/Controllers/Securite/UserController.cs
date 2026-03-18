using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNetTemplateClean.Application;


namespace DotNetTemplateClean.WebAPI;


[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]

public class UserController(
    IUserService UserService,        
    IMapper Mapper) : ApiBaseController
    {
            
    [Authorize(Roles = "Admin")]
    [HttpPost("CreateUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateViewModel userModel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        var userDto = Mapper.Map<UserCreationDto>(userModel);

        var result = await UserService.CreateUserWithRoleAsync(userDto);
            
        return HandleResult(result);

    }

    // POST: api/user/search
    [Authorize(Roles = "Admin")]
    [HttpPost("UserSearch")]
    public async Task<ActionResult<PagedResult<UserSearchResultDto>>> UserSearch([FromBody] SearchViewModel searchUser)
    {
        var usersSearchResult = await UserService.GetUsersBySearchModel(searchUser);

        return Ok(usersSearchResult);
    }       


    // DELETE: api/user/{id}        
    [Authorize(Roles ="Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // On récupère l'ID utilisateur depuis les claims
        var currentUserId = User.FindFirst("uid")?.Value;

        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        // On continue le flux normal
        var result = await UserService.DeleteUserAsync(id, currentUserId);
        return HandleResult(result);
    }

    // PUT: api/user
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(string id,[FromBody] UpdateUserViewModel updatedUser, CancellationToken ct)
    {
        if(updatedUser == null)
            return BadRequest("Le corps de la requête est vide ou mal formé.");

        // Sécurité : on vérifie la cohérence
        if (id != updatedUser.UserId)
            return BadRequest("L'ID de l'URL ne correspond pas à l'ID du corps de la requête.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userUpdatedDto = Mapper.Map<UserUpdateDto>(updatedUser);

        var result = await UserService.UpdateUserAsync(userUpdatedDto, ct);

        return HandleResult(result);
    }

    // POST: api/user/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // On récupère l'ID utilisateur depuis les claims
        var userId = User.FindFirst("uid")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await UserService.ChangePasswordAsync(userId, model);

       return HandleResult(result);
    }

    [Authorize(Roles = "Admin")] 
    [HttpPost("{id}/unlock")]
    public async Task<ActionResult> Unlock(string id)
    {
        var result = await UserService.UnlockUserAsync(id);

        // HandleResult gérera le 404, le 400 ou le 200/204
        return HandleResult(result);
    }
}
