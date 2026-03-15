

namespace DotNetTemplateClean.Application;

public interface IUserService
{

    Task<PagedResult<UserSearchResultDto>> GetUsersBySearchModel(SearchViewModel searchModel);        
    Task<ServiceResult<string>> CreateUserWithRoleAsync(UserCreationDto dto);
    Task<ServiceResult<object?>> DeleteUserAsync(string id, string currentUserId);
    Task<ServiceResult<string>> UpdateUserAsync(UserUpdateDto userUpdatedDto);
    Task<ServiceResult<object?>> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    Task<ServiceResult<bool>> UnlockUserAsync(string userId);
    Task<ServiceResult<ProfilViewModel>> GetUserProfileAsync(string userId, string userRole);
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginViewModel model);
    Task<ServiceResult<bool>> LogoutAsync();       
}
