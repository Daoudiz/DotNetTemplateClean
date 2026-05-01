using AutoMapper;
using DotNetTemplateClean.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [Authorize(Roles = "Admin")]
    [HttpPost("UserSearch")]
    public async Task<ActionResult<PagedResult<UserSearchResultDto>>> UserSearch([FromBody] SearchViewModel searchUser)
    {
        var usersSearchResult = await UserService.GetUsersBySearchModel(searchUser);
        return Ok(usersSearchResult);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

        var result = await UserService.DeleteUserAsync(id, currentUserId);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserViewModel updatedUser, CancellationToken ct)
    {
        if (updatedUser == null)
            return BadRequest("Le corps de la requete est vide ou mal forme.");

        if (id != updatedUser.UserId)
            return BadRequest("L'ID de l'URL ne correspond pas a l'ID du corps de la requete.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userUpdatedDto = Mapper.Map<UserUpdateDto>(updatedUser);
        var result = await UserService.UpdateUserAsync(userUpdatedDto, ct);
        return HandleResult(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] AdminResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await UserService.AdminResetPasswordAsync(id, model);
        return HandleResult(result);
    }
}
